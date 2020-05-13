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

    public static List<AirQualityMeasurement> ConvertToAirQualityMeasurements(
      this List<AveragedValues> averagedValues, short installationId, DateTime requestTime)
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
          response = webClient.DownloadString(config.GetValue<string>("AppSettings:AirlyApi:MeasurementsUri")
            + currentInstallationId.ToString());

          yield return JsonConvert.DeserializeObject<Measurements>(response);
        }
      }
    }

    public static void SaveNewMeasurements(
      this AirlyContext context,
      List<AirQualityMeasurement> history,
      short installationId,
      short minNumberOfMeasurements)
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

      if (history.Count >= minNumberOfMeasurements)
      {
        context.ArchiveMeasurements.AddRange(history);
        context.SaveChanges();
      }
    }

    public static void SaveNewForecasts(
      this AirlyContext context,
      List<AirQualityForecast> forecast,
      short installationId,
      short minNumberOfMeasurements)
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

      if (forecast.Count >= minNumberOfMeasurements)
      {
        context.ArchiveForecasts.AddRange(forecast);
        context.SaveChanges();
      }
    }

    public static void CalculateNewMeasurementsRange(
      List<AirQualityMeasurement> archiveMeasurements,
      List<AirQualityForecast> archiveForecasts,
      AirQualityForecastError lastForecastError,
      out int measurementsStartIndex,
      out int forecastsStartIndex,
      out int numberOfElements)
    {
      int i = archiveMeasurements.Count - 1;
      while (i >= 0 && lastForecastError.TillDateTime < archiveMeasurements[i].TillDateTime)
      {
        i--;
      }
      measurementsStartIndex = ++i;

      i = archiveForecasts.Count - 1;
      while (i >= 0 && lastForecastError.TillDateTime < archiveForecasts[i].TillDateTime)
      {
        i--;
      }
      forecastsStartIndex = ++i;

      numberOfElements = archiveMeasurements.Count - measurementsStartIndex;
    }

    public static List<AirQualityForecastError> CalculateForecastErrors(
      this List<AirQualityForecast> archiveForecasts,
      List<AirQualityMeasurement> archiveMeasurements,
      short installationId)
    {
      int dailyCounter = 0;
      int firstForecastItemIndex = 0;

      int caqiErrorsDailySum = 0, pm25ErrorsDailySum = 0, pm10ErrorsDailySum = 0;
      int caqiErrorsTotalSum = 0, pm25ErrorsTotalSum = 0, pm10ErrorsTotalSum = 0;
      int i = 0, j = 0;

      var forecastErrors = new List<AirQualityForecastError>();
      AirQualityForecastError dailyError;

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

          var error = new AirQualityForecastError
          {
            InstallationId = installationId,
            FromDateTime = archiveMeasurements[i].FromDateTime,
            TillDateTime = archiveMeasurements[i].TillDateTime,
            AirlyCaqiPctError = Convert.ToInt16(airlyCaqiRelativeError * 100),
            Pm25PctError = Convert.ToInt16(pm25RelativeError * 100),
            Pm10PctError = Convert.ToInt16(pm10RelativeError * 100),
            RequestDateTime = archiveMeasurements[i].RequestDateTime,
          };

          forecastErrors.Add(error);

          caqiErrorsDailySum += Math.Abs(error.AirlyCaqiPctError);
          pm25ErrorsDailySum += Math.Abs(error.Pm25PctError);
          pm10ErrorsDailySum += Math.Abs(error.Pm10PctError);

          if (i != 0 && archiveMeasurements[i].RequestDateTime != archiveMeasurements[i - 1].RequestDateTime)
          {
            // Calculate MAPE of daily forecast
            if (dailyCounter >= 23)
            {
              dailyError = generateForecastError(
                caqiErrorsDailySum, pm25ErrorsDailySum, pm10ErrorsDailySum, dailyCounter);
              forecastErrors.Add(dailyError);
            }

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
          archiveMeasurements.RemoveAt(i);
        }
        else
        {
          archiveForecasts.RemoveAt(j);
        }
      }

      if (dailyCounter >= 23)
      {
        var lastDailyError = generateForecastError(
          caqiErrorsDailySum, pm25ErrorsDailySum, pm10ErrorsDailySum, dailyCounter);
        forecastErrors.Add(lastDailyError);
      }

      return forecastErrors;

      AirQualityForecastError generateForecastError(
        int caqiErrorsSum, int pm25ErrorsSum, int pm10ErrorsSum, int counter)
      {
        return new AirQualityForecastError
        {
          InstallationId = installationId,
          FromDateTime = archiveMeasurements[firstForecastItemIndex].FromDateTime,
          TillDateTime = archiveMeasurements[i - 1].TillDateTime,
          AirlyCaqiPctError = (short)(caqiErrorsSum / counter),
          Pm25PctError = (short)(pm25ErrorsSum / counter),
          Pm10PctError = (short)(pm10ErrorsSum / counter),
          RequestDateTime = archiveMeasurements[i - 1].RequestDateTime,
        };
      }
    }
  }
}
