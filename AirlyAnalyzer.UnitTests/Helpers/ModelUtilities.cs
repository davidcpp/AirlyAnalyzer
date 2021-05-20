namespace AirlyAnalyzer.UnitTests.Helpers
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.Models;

  using EP = AirlyAnalyzer.Models.ForecastErrorPeriod;

  public static class ModelUtilities
  {
    private const short _idForAllInstallations = -1;
    public const byte RequestMinutesOffset = 30;

    public static InstallationInfo GetTestInstallationInfo(
        short installationId, DateTime updateDate)
    {
      return new InstallationInfo
      {
        InstallationId = installationId,
        UpdateDate = updateDate,
        Address = new Address()
        {
          City = "Pniewy",
          Country = "Poland",
          Street = "Poznańska",
          Number = "15",
        }
      };
    }

    public static IEnumerable<AirQualityMeasurement> GenerateMeasurements(
        short installationId,
        DateTime startDate,
        int numberOfMeasurements)
    {
      for (int i = 0; i < numberOfMeasurements; i++)
      {
        yield return CreateMeasurement(
            installationId,
            startDate.AddHours(i),
            startDate.AddHours(numberOfMeasurements)
                     .AddMinutes(RequestMinutesOffset));
      }
    }

    public static IEnumerable<AirQualityMeasurement> GenerateMeasurements(
        short installationId,
        DateTime startDate,
        short numberOfDays,
        short numberOfMeasurementsInDay)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfMeasurementsInDay; j++)
        {
          yield return CreateMeasurement(
              installationId,
              startDate.AddHours(j),
              startDate.AddDays(1).AddMinutes(RequestMinutesOffset));
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
        int numberOfForecasts)
    {
      for (int i = 0; i < numberOfForecasts; i++)
      {
        yield return CreateForecast(
            installationId,
            startDate.AddHours(i),
            startDate.AddHours(numberOfForecasts)
                     .AddMinutes(RequestMinutesOffset));
      }
    }

    public static IEnumerable<AirQualityForecast> GenerateForecasts(
        short installationId,
        DateTime startDate,
        short numberOfDays,
        short numberOfForecastsInDay,
        AirQualityDataSource source = AirQualityDataSource.Airly)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfForecastsInDay; j++)
        {
          yield return CreateForecast(
              installationId,
              startDate.AddHours(j),
              startDate.AddDays(1).AddMinutes(RequestMinutesOffset),
              source: source);
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
        short pm10 = 1,
        AirQualityDataSource source = AirQualityDataSource.Airly)
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
        Source = source,
      };
    }

    public static IEnumerable<AirQualityForecastError>
        GenerateHourlyForecastErrors(
            short installationId,
            DateTime startDate,
            int numberOfForecastErrors,
            short durationInHours = 1)
    {
      for (int i = 0; i < numberOfForecastErrors; i++)
      {
        yield return CreateForecastError(
            installationId,
            EP.Hour,
            startDate.AddHours(i),
            startDate.AddHours(numberOfForecastErrors)
                     .AddMinutes(RequestMinutesOffset),
            durationInHours);
      }
    }

    public static IEnumerable<AirQualityForecastError>
        GenerateHourlyForecastErrors(
            short installationId,
            DateTime startDate,
            short numberOfDays,
            short numberOfForecastErrorsInDay)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        short requestInterval
            = i % 2 == 0 ? numberOfForecastErrorsInDay : (short)24;

        for (int j = 0; j < numberOfForecastErrorsInDay; j++)
        {
          yield return CreateForecastError(
              installationId,
              EP.Hour,
              startDate.AddHours(j),
              startDate.AddHours(requestInterval)
                       .AddMinutes(RequestMinutesOffset));
        }
        startDate = startDate.AddDays(1);
      }
    }

    public static IEnumerable<AirQualityForecastError>
        GenerateDailyForecastErrors(
            short installationId,
            DateTime startDate,
            int numberOfForecastErrors,
            short durationInHours = 24)
    {
      for (int i = 0; i < numberOfForecastErrors; i++)
      {
        short requestInterval = i % 2 == 0 ? durationInHours : (short)24;

        yield return CreateForecastError(
            installationId,
            EP.Day,
            startDate.AddDays(i),
            startDate.AddDays(i).AddHours(requestInterval)
                                .AddMinutes(RequestMinutesOffset),
            durationInHours);
      }
    }

    public static IEnumerable<AirQualityForecastError> GenerateTotalForecastErrors(
        List<short> _installationIds, DateTime startDate, short numberOfDays)
    {
      var requestDate = startDate.AddDays(numberOfDays)
                                 .AddMinutes(RequestMinutesOffset);

      int durationInHours = numberOfDays * 24;

      foreach (short installationId in _installationIds)
      {
        yield return CreateForecastError(
            installationId, EP.Total, startDate, requestDate, durationInHours);
      }

      yield return CreateForecastError(
          _idForAllInstallations, EP.Total, startDate, requestDate, durationInHours);
    }

    public static AirQualityForecastError CreateForecastError(
        short installationId,
        ForecastErrorPeriod errorPeriod,
        DateTime forecastErrorDate,
        DateTime requestDate,
        int durationInHours = 1)
    {
      return new AirQualityForecastError
      {
        InstallationId = installationId,
        FromDateTime = forecastErrorDate,
        TillDateTime = forecastErrorDate.AddHours(durationInHours),
        AirlyCaqi = 1,
        AirlyCaqiPct = 1,
        Pm25 = 1,
        Pm25Pct = 1,
        Pm10 = 1,
        Pm10Pct = 1,
        RequestDateTime = requestDate,
        Period = errorPeriod,
      };
    }
  }
}
