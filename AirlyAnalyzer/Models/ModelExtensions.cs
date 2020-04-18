using AirlyAnalyzer.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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

    public static Measurements DownloadInstallationMeasurements(IConfiguration config)
    {
      string response;

      using (var webClient = new WebClient())
      {
        webClient.BaseAddress = config.GetValue<string>("AppSettings:AirlyApi:Uri");
        webClient.Headers.Remove(HttpRequestHeader.Accept);
        webClient.Headers.Add(HttpRequestHeader.Accept, config.GetValue<string>("AirlyApi:ContentType"));
        webClient.Headers.Add(AirlyApiKeyHeaderName, config.GetValue<string>("AppSettings:AirlyApi:Key"));
        response = webClient.DownloadString(config.GetValue<string>("AppSettings:AirlyApi:MeasurementsUri") +
          config.GetValue<int>("AppSettings:AirlyApi:InstallationId").ToString());
      }

      return JsonConvert.DeserializeObject<Measurements>(response);
    }

    public static void SaveNewMeasurements(
      this AirlyContext context,
      List<AirQualityMeasurement> history,
      DateTime requestTime)
    {
      // Check if some of measurements there already are in Database
      if (context.ArchiveMeasurements.Count() > 0)
      {
        var dbLastElement = context.ArchiveMeasurements.ToList().Last();
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
      DateTime requestTime)
    {
      // Check if some of forecasts there already are in Database
      if (context.ArchiveForecasts.Count() > 0)
      {
        var dbLastElement = context.ArchiveForecasts.ToList().Last();
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

    public static List<AirQualityForecastAccuracy> CalculateForecastAccuracy(
      this List<AirQualityForecast> archiveForecasts,
      List<AirQualityMeasurement> archiveMeasurements,
      short installationId)
    {
      var forecastAccuracyRates = new List<AirQualityForecastAccuracy>();

      for (int i = 0, j = 0; j < archiveMeasurements.Count && i < archiveForecasts.Count;)
      {
        var currentForecastDateTime = archiveForecasts[i].TillDateTime.ToUniversalTime();
        var currentMeasurementDateTime = archiveMeasurements[j].TillDateTime.ToUniversalTime();

        if (currentForecastDateTime == currentMeasurementDateTime)
        {
          double airlyCaqiRelativeError =
            (double)(archiveMeasurements[j].AirlyCaqi - archiveForecasts[i].AirlyCaqi)
            / (double)archiveMeasurements[j].AirlyCaqi;

          double pm25RelativeError =
            (double)(archiveMeasurements[j].Pm25 - archiveForecasts[i].Pm25)
            / (double)archiveMeasurements[j].Pm25;

          double pm10RelativeError =
            (double)(archiveMeasurements[j].Pm10 - archiveForecasts[i].Pm10)
            / (double)archiveMeasurements[j].Pm10;

          var accuracyRate = new AirQualityForecastAccuracy
          {
            FromDateTime = archiveMeasurements[j].FromDateTime,
            TillDateTime = archiveMeasurements[j].TillDateTime,
            ForecastRequestDateTime = archiveForecasts[i].RequestDateTime,
            InstallationId = installationId,
            AirlyCaqiError = Convert.ToInt16(airlyCaqiRelativeError * 100),
            Pm25Error = Convert.ToInt16(pm25RelativeError * 100),
            Pm10Error = Convert.ToInt16(pm10RelativeError * 100),
          };

          forecastAccuracyRates.Add(accuracyRate);
          i++; j++;
        }
        else if (currentForecastDateTime > currentMeasurementDateTime)
        {
          j++;
        }
        else
        {
          i++;
        }
      }

      return forecastAccuracyRates;
    }
  }
}
