using AirlyAnalyzer.Models;
using System;
using System.Collections.Generic;

namespace AirlyAnalyzer.Tests.Models
{
  public static class AuxiliaryMethods
  {
    internal static IEnumerable<AirQualityMeasurement> GenerateMeasurements(
      DateTime startDate, int numberOfMeasurements, byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfMeasurements; i++)
      {
        yield return CreateMeasurement(
          startDate.AddHours(i),
          startDate.AddHours(numberOfMeasurements).AddMinutes(requestMinutesOffset));
      }
    }

    internal static IEnumerable<AirQualityMeasurement> GenerateMeasurements(
      DateTime startDate, short numberOfDays, short numberOfMeasurementsInDay, byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfMeasurementsInDay; j++)
        {
          yield return CreateMeasurement(
            startDate.AddHours(j),
            startDate.AddHours(numberOfMeasurementsInDay).AddMinutes(requestMinutesOffset));
        }
        startDate = startDate.AddHours(numberOfMeasurementsInDay);
      }
    }

    private static AirQualityMeasurement CreateMeasurement(DateTime measurementDate, DateTime requestDate)
    {
      return new AirQualityMeasurement
      {
        FromDateTime = measurementDate,
        TillDateTime = measurementDate.AddHours(1),
        RequestDateTime = requestDate,
        InstallationId = 1001,
        AirlyCaqi = 100,
        Pm1 = 10,
        Pm25 = 20,
        Pm10 = 30,
        Humidity = 50,
        Pressure = 1013,
        Temperature = 15,
      };
    }

    internal static IEnumerable<AirQualityForecast> GenerateForecasts(
      DateTime startDate, int numberOfForecasts, short requestMinutesOffset)
    {
      for (int i = 0; i < numberOfForecasts; i++)
      {
        yield return CreateForecast(
          startDate.AddHours(i),
          startDate.AddHours(numberOfForecasts).AddMinutes(requestMinutesOffset));
      }
    }

    internal static IEnumerable<AirQualityForecast> GenerateForecasts(
      DateTime startDate, short numberOfDays, short numberOfForecastsInDay, byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfForecastsInDay; j++)
        {
          yield return CreateForecast(
            startDate.AddHours(j),
            startDate.AddHours(numberOfForecastsInDay).AddMinutes(requestMinutesOffset));
        }
        startDate = startDate.AddHours(numberOfForecastsInDay);
      }
    }

    private static AirQualityForecast CreateForecast(DateTime forecastDate, DateTime startDate)
    {
      return new AirQualityForecast
      {
        FromDateTime = forecastDate,
        TillDateTime = forecastDate.AddHours(1),
        RequestDateTime = startDate,
        InstallationId = 1001,
        AirlyCaqi = 100,
        Pm25 = 20,
        Pm10 = 30,
      };
    }
  }
}
