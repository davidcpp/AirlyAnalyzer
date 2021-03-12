namespace AirlyAnalyzer.Tests.Models
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;

  using ET = AirlyAnalyzer.Models.ForecastErrorType;

  public static class AuxiliaryMethods
  {
    public const byte RequestMinutesOffset = 30;

    public static void AddElementsToDatabase(
        this AirlyContext context,
        List<short> _installationIds,
        DateTime startDate,
        short numberOfDays,
        short numberOfElementsInDay)
    {
      var requestDate = startDate.AddDays(numberOfDays)
                                 .AddMinutes(RequestMinutesOffset);

      for (int i = 0; i < _installationIds.Count; i++)
      {
        short installationId = _installationIds[i];

        context.AddMeasurementsToDatabase(
            installationId, startDate, numberOfDays, numberOfElementsInDay);

        context.AddForecastsToDatabase(
            installationId, startDate, numberOfDays, numberOfElementsInDay);

        context.ForecastErrors.AddRange(
            GenerateHourlyForecastErrors(
                installationId, startDate, numberOfDays, numberOfElementsInDay));

        context.ForecastErrors.AddRange(
            GenerateDailyForecastErrors(
                installationId, startDate, numberOfDays, numberOfElementsInDay));

        int totalErrorDuration = ((numberOfDays - 1) * 24) + numberOfElementsInDay;

        context.ForecastErrors.Add(
            CreateForecastError(
                installationId,
                ET.Total,
                startDate,
                requestDate,
                totalErrorDuration));
      }

      context.SaveChanges();
    }

    public static void AddAllMeasurementsToDatabase(
        this AirlyContext context,
        List<short> _installationIds,
        DateTime startDate,
        short numberOfDays,
        short numberOfMeasurementsInDay)
    {
      for (int i = 0; i < _installationIds.Count; i++)
      {
        context.AddMeasurementsToDatabase(
            _installationIds[i],
            startDate,
            numberOfDays,
            numberOfMeasurementsInDay);
      }
    }

    public static void AddAllForecastsToDatabase(
        this AirlyContext context,
        List<short> _installationIds,
        DateTime startDate,
        short numberOfDays,
        short numberOfForecastsInDay)
    {
      for (int i = 0; i < _installationIds.Count; i++)
      {
        context.AddForecastsToDatabase(
            _installationIds[i],
            startDate,
            numberOfDays,
            numberOfForecastsInDay);
      }
    }

    public static void AddMeasurementsToDatabase(
        this AirlyContext context,
        short installationId,
        DateTime startDate,
        int numberOfMeasurements)
    {
      context.ArchiveMeasurements.AddRange(
          GenerateMeasurements(
              installationId, startDate, numberOfMeasurements));

      context.SaveChanges();
    }

    public static void AddMeasurementsToDatabase(
        this AirlyContext context,
        short installationId,
        DateTime startDate,
        short numberOfDays,
        short numberOfMeasurementsInDay)
    {
      context.ArchiveMeasurements.AddRange(
          GenerateMeasurements(
              installationId, startDate, numberOfDays, numberOfMeasurementsInDay));

      context.SaveChanges();
    }

    public static void AddForecastsToDatabase(
        this AirlyContext context,
        short installationId,
        DateTime startDate,
        int numberOfForecasts)
    {
      context.ArchiveForecasts.AddRange(
          GenerateForecasts(
              installationId, startDate, numberOfForecasts));

      context.SaveChanges();
    }

    public static void AddForecastsToDatabase(
        this AirlyContext context,
        short installationId,
        DateTime startDate,
        short numberOfDays,
        short numberOfForecastsInDay)
    {
      context.ArchiveForecasts.AddRange(
          GenerateForecasts(
              installationId, startDate, numberOfDays, numberOfForecastsInDay));

      context.SaveChanges();
    }

    public static void AddHourlyForecastErrorsToDatabase(
        this AirlyContext context,
        short installationId,
        DateTime startDate,
        int numberOfForecastErrors)
    {
      context.ForecastErrors.AddRange(
          GenerateHourlyForecastErrors(
              installationId, startDate, numberOfForecastErrors));

      context.SaveChanges();
    }

    public static void AddDailyForecastErrorsToDatabase(
        this AirlyContext context,
        short installationId,
        DateTime startDate,
        short numberOfDays,
        short durationInHours = 24)
    {
      context.ForecastErrors.AddRange(GenerateDailyForecastErrors(
          installationId, startDate, numberOfDays, durationInHours));

      context.SaveChanges();
    }

    public static void AddTotalForecastErrorsToDatabase(
        this AirlyContext context,
        List<short> _installationIds,
        DateTime startDate,
        short numberOfDays)
    {
      context.ForecastErrors.AddRange(GenerateTotalForecastErrors(
          _installationIds, startDate, numberOfDays));
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
        short numberOfForecastsInDay)
    {
      for (int i = 0; i < numberOfDays; i++)
      {
        for (int j = 0; j < numberOfForecastsInDay; j++)
        {
          yield return CreateForecast(
              installationId,
              startDate.AddHours(j),
              startDate.AddDays(1).AddMinutes(RequestMinutesOffset));
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
            short durationInHours = 1)
    {
      for (int i = 0; i < numberOfForecastErrors; i++)
      {
        yield return CreateForecastError(
            installationId,
            ET.Hourly,
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
              ET.Hourly,
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
            ET.Daily,
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
            installationId, ET.Total, startDate, requestDate, durationInHours);
      }

      yield return CreateForecastError(
          -1, ET.Total, startDate, requestDate, durationInHours);
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
        AirlyCaqi = 1,
        AirlyCaqiPct = 1,
        Pm25 = 1,
        Pm25Pct = 1,
        Pm10 = 1,
        Pm10Pct = 1,
        RequestDateTime = requestDate,
        ErrorType = errorType,
      };
    }

    public static void Seed(AirlyContext context)
    {
      context.ArchiveMeasurements.RemoveRange(context.ArchiveMeasurements);
      context.ArchiveForecasts.RemoveRange(context.ArchiveForecasts);
      context.ForecastErrors.RemoveRange(context.ForecastErrors);
      context.SaveChanges();
    }
  }
}
