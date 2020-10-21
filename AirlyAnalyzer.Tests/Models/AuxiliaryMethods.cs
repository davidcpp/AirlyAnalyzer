using AirlyAnalyzer.Models;
using System;
using System.Collections.Generic;

namespace AirlyAnalyzer.Tests.Models
{
  public static class AuxiliaryMethods
  {
    public static IEnumerable<AirQualityMeasurement> GenerateMeasurements(
      short installationId, DateTime startDate, int numberOfMeasurements, byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfMeasurements; i++)
      {
        yield return CreateMeasurement(
          installationId,
          startDate.AddHours(i),
          startDate.AddHours(numberOfMeasurements).AddMinutes(requestMinutesOffset));
      }
    }

    public static IEnumerable<AirQualityMeasurement> GenerateMeasurements(short installationId,
      DateTime startDate, short numberOfDays, short numberOfMeasurementsInDay, byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfMeasurementsInDay; j++)
        {
          yield return CreateMeasurement(
            installationId,
            startDate.AddHours(j),
            startDate.AddHours(numberOfMeasurementsInDay).AddMinutes(requestMinutesOffset));
        }
        startDate = startDate.AddHours(numberOfMeasurementsInDay);
      }
    }

    public static AirQualityMeasurement CreateMeasurement(short installationId, DateTime measurementDate,
      DateTime requestDate, byte airlyCaqi = 1, short pm25 = 1, short pm10 = 1)
    {
      return new AirQualityMeasurement
      {
        FromDateTime = measurementDate,
        TillDateTime = measurementDate.AddHours(1),
        RequestDateTime = requestDate,
        InstallationId = installationId,
        AirlyCaqi = airlyCaqi,
        Pm1 = 1,
        Pm25 = pm25,
        Pm10 = pm10,
        Humidity = 1,
        Pressure = 1,
        Temperature = 1,
      };
    }

    public static IEnumerable<AirQualityForecast> GenerateForecasts(
      short installationId, DateTime startDate, int numberOfForecasts, short requestMinutesOffset)
    {
      for (int i = 0; i < numberOfForecasts; i++)
      {
        yield return CreateForecast(
          installationId,
          startDate.AddHours(i),
          startDate.AddHours(numberOfForecasts).AddMinutes(requestMinutesOffset));
      }
    }

    public static IEnumerable<AirQualityForecast> GenerateForecasts(short installationId,
      DateTime startDate, short numberOfDays, short numberOfForecastsInDay, byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfForecastsInDay; j++)
        {
          yield return CreateForecast(
            installationId,
            startDate.AddHours(j),
            startDate.AddHours(numberOfForecastsInDay).AddMinutes(requestMinutesOffset));
        }
        startDate = startDate.AddHours(numberOfForecastsInDay);
      }
    }

    public static AirQualityForecast CreateForecast(short installationId, DateTime forecastDate,
      DateTime requestDate, byte airlyCaqi = 1, short pm25 = 1, short pm10 = 1)
    {
      return new AirQualityForecast
      {
        FromDateTime = forecastDate,
        TillDateTime = forecastDate.AddHours(1),
        RequestDateTime = requestDate,
        InstallationId = installationId,
        AirlyCaqi = airlyCaqi,
        Pm25 = pm25,
        Pm10 = pm10,
      };
    }
  }
}
