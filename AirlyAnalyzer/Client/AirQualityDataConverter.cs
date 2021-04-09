namespace AirlyAnalyzer.Client
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.Models;

  public static class AirQualityDataConverter
  {
    public static List<AirQualityMeasurement> ConvertToAirQualityMeasurements(
        this List<AveragedValues> averagedValues,
        short installationId,
        Address installationAddress,
        DateTime requestTime)
    {
      var airQualityMeasurements = new List<AirQualityMeasurement>();

      foreach (var averagedValue in averagedValues)
      {
        var measurement = (AirQualityMeasurement)averagedValue;
        measurement.InstallationId = installationId;
        measurement.InstallationAddress = installationAddress?.ToString();
        measurement.RequestDateTime = requestTime;

        airQualityMeasurements.Add(measurement);
      }

      return airQualityMeasurements;
    }

    public static List<AirQualityForecast> ConvertToAirQualityForecasts(
        this List<AveragedValues> averagedValues,
        short installationId,
        Address installationAddress,
        DateTime requestTime)
    {
      var airQualityForecasts = new List<AirQualityForecast>();

      foreach (var averagedValue in averagedValues)
      {
        var forecast = (AirQualityForecast)averagedValue;
        forecast.InstallationId = installationId;
        forecast.InstallationAddress = installationAddress?.ToString();
        forecast.RequestDateTime = requestTime;

        airQualityForecasts.Add(forecast);
      }

      return airQualityForecasts;
    }
  }
}
