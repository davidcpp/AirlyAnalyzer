using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AirlyAnalyzer.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AirlyAnalyzer.Models
{
  public static class ModelExtensions
  {
    private const string AirlyApiKeyHeaderName = "apikey";

    public static List<AirQualityMeasurement>
      ConvertToAirQualityMeasurements(this List<AveragedValues> averagedValues, short installationId, DateTime requestTime)
    {
      var airQualityMeasurements = new List<AirQualityMeasurement>();

      foreach (var averagedValue in averagedValues)
      {
        var measurement = (AirQualityMeasurement)averagedValue;
        measurement.InstallationId = installationId;
        measurement.RequestDateTime = requestTime;
        airQualityMeasurements.Add(measurement);
      }

      return airQualityMeasurements;
    }

    public static List<AirQualityForecast> ConvertToAirQualityForecasts(
      this List<AveragedValues> averagedValues,
      short installationId,
      DateTime requestTime)
    {
      var airQualityForecasts = new List<AirQualityForecast>();

      foreach (var averagedValue in averagedValues)
      {
        var forecast = (AirQualityForecast)averagedValue;
        forecast.InstallationId = installationId;
        forecast.RequestDateTime = requestTime;
        airQualityForecasts.Add(forecast);
      }

      return airQualityForecasts;
    }

    public static IEnumerable<Measurements> DownloadInstallationMeasurements(
      IConfiguration config,
      List<short> installationIDsList)
    {
      string response;

      for (int i = 0; i < installationIDsList.Count; i++)
      {
        using (var webClient = new WebClient())
        {
          short currentInstallationId = config.GetValue<short>($"AppSettings:AirlyApi:InstallationIds:{i}");

          webClient.BaseAddress = config.GetValue<string>("AppSettings:AirlyApi:Uri");
          webClient.Headers.Remove(HttpRequestHeader.Accept);
          webClient.Headers.Add(HttpRequestHeader.Accept, config.GetValue<string>("AirlyApi:ContentType"));
          webClient.Headers.Add(AirlyApiKeyHeaderName, config.GetValue<string>("AppSettings:AirlyApi:Key"));
          response = webClient.DownloadString(config.GetValue<string>("AppSettings:AirlyApi:MeasurementsUri") +
            currentInstallationId.ToString());

          yield return JsonConvert.DeserializeObject<Measurements>(response);
        }
      }
    }

    public static void SaveNewMeasurements(
      this AirlyContext context,
      List<AirQualityMeasurement> history,
      DateTime requestTime,
      short installationId)
    {
      var archiveMeasurementsForInstallation =
        context.ArchiveMeasurements.Where(x => x.InstallationId == installationId).ToList();

      // Check if some of measurements there already are in Database
      if (archiveMeasurementsForInstallation.Count > 0)
      {
        var dbLastElement = archiveMeasurementsForInstallation.Last();
        while (history.Count > 0 && history[0].FromDateTime <= dbLastElement.FromDateTime)
        {
          history.RemoveAt(0);
        }
      }

      if (requestTime.Hour >= 19 && history.Count >= 23)
      {
        context.ArchiveMeasurements.AddRange(history);
        context.SaveChanges();
      }
    }

    public static void SaveNewForecasts(
      this AirlyContext context,
      List<AirQualityForecast> forecast,
      DateTime requestTime,
      short installationId)
    {
      var archiveForecastsForInstallation =
        context.ArchiveForecasts.Where(x => x.InstallationId == installationId).ToList();

      // Check if some of forecasts there already are in Database
      if (archiveForecastsForInstallation.Count > 0)
      {
        var dbLastElement = archiveForecastsForInstallation.Last();
        while (forecast.Count > 0 && forecast[0].FromDateTime <= dbLastElement.FromDateTime)
        {
          forecast.RemoveAt(0);
        }
      }

      if (requestTime.Hour >= 19 && forecast.Count >= 23)
      {
        context.ArchiveForecasts.AddRange(forecast);
        context.SaveChanges();
      }
    }

    public static void CalculateNewMeasurementsRange(
      List<AirQualityMeasurement> archiveMeasurements,
      List<AirQualityForecast> archiveForecasts,
      AirQualityForecastAccuracy lastForecastAccuracy,
      out int measurementsStartIndex,
      out int forecastsStartIndex,
      out int numberOfElements)
    {
      measurementsStartIndex = archiveMeasurements.Count - 1;
      while (measurementsStartIndex >= 0 && lastForecastAccuracy.TillDateTime < archiveMeasurements[measurementsStartIndex].TillDateTime)
      {
        measurementsStartIndex--;
      }
      measurementsStartIndex++;

      forecastsStartIndex = archiveForecasts.Count - 1;
      while (forecastsStartIndex >= 0 && lastForecastAccuracy.TillDateTime < archiveForecasts[forecastsStartIndex].TillDateTime)
      {
        forecastsStartIndex--;
      }
      forecastsStartIndex++;

      numberOfElements = archiveMeasurements.Count - measurementsStartIndex;
    }

    public static List<AirQualityForecastAccuracy> CalculateForecastAccuracy(
      this List<AirQualityForecast> archiveForecasts,
      List<AirQualityMeasurement> archiveMeasurements,
      short installationId)
    {
      int dailyCounter = 0, counter = 0;
      int firstForecastItemIndex = 0;

      int caqiErrorsDailySum = 0, pm25ErrorsDailySum = 0, pm10ErrorsDailySum = 0;
      int caqiErrorsTotalSum = 0, pm25ErrorsTotalSum = 0, pm10ErrorsTotalSum = 0;
      int i = 0, j = 0;

      var forecastAccuracyRates = new List<AirQualityForecastAccuracy>();
      AirQualityForecastAccuracy dailyAccuracyRate;

      for (; i < archiveMeasurements.Count && j < archiveForecasts.Count;)
      {
        var currentMeasurementDateTime = archiveMeasurements[i].TillDateTime.ToUniversalTime();
        var currentForecastDateTime = archiveForecasts[j].TillDateTime.ToUniversalTime();

        if (currentForecastDateTime == currentMeasurementDateTime)
        {
          double pm25RelativeError =
            (double)(archiveMeasurements[i].Pm25 - archiveForecasts[j].Pm25)
            / (double)archiveMeasurements[i].Pm25;

          double pm10RelativeError =
            (double)(archiveMeasurements[i].Pm10 - archiveForecasts[j].Pm10)
            / (double)archiveMeasurements[i].Pm10;

          double airlyCaqiRelativeError =
            (double)(archiveMeasurements[i].AirlyCaqi - archiveForecasts[j].AirlyCaqi)
            / (double)archiveMeasurements[i].AirlyCaqi;

          var accuracyRate = new AirQualityForecastAccuracy
          {
            InstallationId = installationId,
            FromDateTime = archiveMeasurements[i].FromDateTime,
            TillDateTime = archiveMeasurements[i].TillDateTime,
            AirlyCaqiError = Convert.ToInt16(airlyCaqiRelativeError * 100),
            Pm25Error = Convert.ToInt16(pm25RelativeError * 100),
            Pm10Error = Convert.ToInt16(pm10RelativeError * 100),
            ForecastRequestDateTime = archiveMeasurements[i].RequestDateTime,
          };

          forecastAccuracyRates.Add(accuracyRate);

          caqiErrorsDailySum += Math.Abs(accuracyRate.AirlyCaqiError);
          pm25ErrorsDailySum += Math.Abs(accuracyRate.Pm25Error);
          pm10ErrorsDailySum += Math.Abs(accuracyRate.Pm10Error);

          if (j != 0 && archiveMeasurements[i].RequestDateTime != archiveMeasurements[i - 1].RequestDateTime)
          {
            // Calculate MAPE of daily forecast
            if (dailyCounter >= 23)
            {
              dailyAccuracyRate = generateForecastAccuracyRate(
                caqiErrorsDailySum, pm25ErrorsDailySum, pm10ErrorsDailySum, dailyCounter);
              forecastAccuracyRates.Add(dailyAccuracyRate);
            }

            counter += dailyCounter;
            caqiErrorsTotalSum += caqiErrorsDailySum;
            pm25ErrorsTotalSum += pm25ErrorsDailySum;
            pm10ErrorsTotalSum += pm10ErrorsDailySum;

            dailyCounter = 0;
            firstForecastItemIndex = i;
            caqiErrorsDailySum = 0;
            pm25ErrorsDailySum = 0;
            pm10ErrorsDailySum = 0;
          }

          dailyCounter++;
          i++; j++;
        }
        else if (currentForecastDateTime > currentMeasurementDateTime)
        {
          i++;
        }
        else
        {
          j++;
        }
      }

      if (dailyCounter >= 23)
      {
        var lastDailyAccuracyRate = generateForecastAccuracyRate(
          caqiErrorsDailySum, pm25ErrorsDailySum, pm10ErrorsDailySum, dailyCounter);
        forecastAccuracyRates.Add(lastDailyAccuracyRate);
      }

      firstForecastItemIndex = 0;
      counter += dailyCounter;
      caqiErrorsTotalSum += caqiErrorsDailySum;
      pm25ErrorsTotalSum += pm25ErrorsDailySum;
      pm10ErrorsTotalSum += pm10ErrorsDailySum;

      // Calculate MAPE of all previous forecasts
      var totalAccuracyRate = generateForecastAccuracyRate(
        caqiErrorsTotalSum, pm25ErrorsTotalSum, pm10ErrorsTotalSum, counter);
      forecastAccuracyRates.Add(totalAccuracyRate);

      return forecastAccuracyRates;

      AirQualityForecastAccuracy generateForecastAccuracyRate(
        int caqiErrorsSum, int pm25ErrorsSum, int pm10ErrorsSum, int counter)
      {
        return new AirQualityForecastAccuracy
        {
          InstallationId = installationId,
          FromDateTime = archiveMeasurements[firstForecastItemIndex].FromDateTime,
          TillDateTime = archiveMeasurements[i - 1].TillDateTime,
          AirlyCaqiError = (short)(caqiErrorsSum / counter),
          Pm25Error = (short)(pm25ErrorsSum / counter),
          Pm10Error = (short)(pm10ErrorsSum / counter),
          ForecastRequestDateTime = archiveMeasurements[i - 1].RequestDateTime,
        };
      }
    }
  }
}
