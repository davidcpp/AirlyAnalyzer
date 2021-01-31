namespace AirlyAnalyzer.Tests.Models
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.Models;

  public static class AuxiliaryMethods
  {
    public static IEnumerable<AirQualityMeasurement> GenerateMeasurements(
        short installationId,
        DateTime startDate,
        int numberOfMeasurements,
        byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfMeasurements; i++)
      {
        yield return CreateMeasurement(
            installationId,
            startDate.AddHours(i),
            startDate.AddHours(numberOfMeasurements)
                     .AddMinutes(requestMinutesOffset));
      }
    }

    public static IEnumerable<AirQualityMeasurement> GenerateMeasurements(
        short installationId,
        DateTime startDate,
        short numberOfDays,
        short numberOfMeasurementsInDay,
        byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfMeasurementsInDay; j++)
        {
          yield return CreateMeasurement(
              installationId,
              startDate.AddHours(j),
              startDate.AddDays(1).AddMinutes(requestMinutesOffset));
        }
        startDate = startDate.AddDays(1);
      }
    }

    public static AirQualityMeasurement CreateMeasurement(
        short installationId,
        DateTime measurementDate,
        DateTime requestDate,
        byte airlyCaqi = 1,
        short pm25 = 1,
        short pm10 = 1)
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
        short installationId,
        DateTime startDate,
        int numberOfForecasts,
        byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfForecasts; i++)
      {
        yield return CreateForecast(
            installationId,
            startDate.AddHours(i),
            startDate.AddHours(numberOfForecasts)
                     .AddMinutes(requestMinutesOffset));
      }
    }

    public static IEnumerable<AirQualityForecast> GenerateForecasts(
        short installationId,
        DateTime startDate,
        short numberOfDays,
        short numberOfForecastsInDay,
        byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfForecastsInDay; j++)
        {
          yield return CreateForecast(
              installationId,
              startDate.AddHours(j),
              startDate.AddDays(1).AddMinutes(requestMinutesOffset));
        }
        startDate = startDate.AddDays(1);
      }
    }

    public static AirQualityForecast CreateForecast(
        short installationId,
        DateTime forecastDate,
        DateTime requestDate,
        byte airlyCaqi = 1,
        short pm25 = 1,
        short pm10 = 1)
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

    public static IEnumerable<AirQualityForecastError>
        GenerateHourlyForecastErrors(
            short installationId,
            DateTime startDate,
            int numberOfForecastErrors,
            byte requestMinutesOffset,
            short durationInHours = 1)
    {
      for (int i = 0; i < numberOfForecastErrors; i++)
      {
        yield return CreateForecastError(
            installationId,
            ForecastErrorType.Hourly,
            startDate.AddHours(i),
            startDate.AddHours(numberOfForecastErrors)
                     .AddMinutes(requestMinutesOffset),
            durationInHours);
      }
    }

    public static IEnumerable<AirQualityForecastError>
        GenerateHourlyForecastErrors(
            short installationId,
            DateTime startDate,
            short numberOfDays,
            short numberOfForecastErrorsInDay,
            byte requestMinutesOffset)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        short requestInterval = i % 2 == 0 ? numberOfForecastErrorsInDay : (short)24;

        for (int j = 0; j < numberOfForecastErrorsInDay; j++)
        {
          yield return CreateForecastError(
              installationId,
              ForecastErrorType.Hourly,
              startDate.AddHours(j),
              startDate.AddHours(requestInterval)
                       .AddMinutes(requestMinutesOffset));
        }
        startDate = startDate.AddDays(1);
      }
    }

    public static IEnumerable<AirQualityForecastError>
        GenerateDailyForecastErrors(
            short installationId,
            DateTime startDate,
            int numberOfForecastErrors,
            byte requestMinutesOffset,
            short durationInHours = 1)
    {
      for (int i = 0; i < numberOfForecastErrors; i++)
      {
        short requestInterval = i % 2 == 0 ? durationInHours : (short)24;

        yield return CreateForecastError(
            installationId,
            ForecastErrorType.Daily,
            startDate.AddDays(i),
            startDate.AddDays(i).AddHours(requestInterval)
                                .AddMinutes(requestMinutesOffset),
            durationInHours);
      }
    }

    public static AirQualityForecastError CreateForecastError(
        short installationId,
        ForecastErrorType errorType,
        DateTime forecastErrorDate,
        DateTime requestDate,
        int durationInHours = 1)
    {
      return new AirQualityForecastError
      {
        InstallationId = installationId,
        FromDateTime = forecastErrorDate,
        TillDateTime = forecastErrorDate.AddHours(durationInHours),
        AirlyCaqiError = 1,
        AirlyCaqiPctError = 1,
        Pm25Error = 1,
        Pm25PctError = 1,
        Pm10Error = 1,
        Pm10PctError = 1,
        RequestDateTime = requestDate,
        ErrorType = errorType,
      };
    }
  }
}
