namespace AirlyAnalyzer.Tests.Models
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.Data;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;

  using ET = AirlyAnalyzer.Models.ForecastErrorType;

  public static class DbUtilities
  {
    public const byte RequestMinutesOffset = 30;

    public static DateTime _startDate;
    public static List<short> _installationIds;

    static DbUtilities()
    {
      _startDate = new DateTime(2000, 1, 1);
      _installationIds = new List<short> { 6306, 6307, 6308, 6309, 6310, 6311 };
    }

    public static void AddElementsToDatabase(
        this AirlyContext context,
        List<short> installationIds,
        DateTime startDate,
        short numberOfDays,
        short numberOfElementsInDay)
    {
      var requestDate = startDate.AddDays(numberOfDays)
                                 .AddMinutes(RequestMinutesOffset);

      foreach (short installationId in installationIds)
      {
        context.AddMeasurementsToDatabase(
            installationId, startDate, numberOfDays, numberOfElementsInDay);

        context.AddForecastsToDatabase(
            installationId, startDate, numberOfDays, numberOfElementsInDay);

        context.ForecastErrors.AddRange(GenerateHourlyForecastErrors(
            installationId, startDate, numberOfDays, numberOfElementsInDay));

        context.ForecastErrors.AddRange(GenerateDailyForecastErrors(
            installationId, startDate, numberOfDays, numberOfElementsInDay));

        int totalErrorDuration = ((numberOfDays - 1) * 24) + numberOfElementsInDay;

        context.ForecastErrors.Add(CreateForecastError(
            installationId, ET.Total, startDate, requestDate, totalErrorDuration));
      }

      context.SaveChanges();
    }

    public static void AddAllMeasurementsToDatabase(
        this AirlyContext context,
        List<short> installationIds,
        DateTime startDate,
        short numberOfDays,
        short numberOfMeasurementsInDay)
    {
      foreach (short installationId in installationIds)
      {
        context.AddMeasurementsToDatabase(
            installationId, startDate, numberOfDays, numberOfMeasurementsInDay);
      }
    }

    public static void AddAllForecastsToDatabase(
        this AirlyContext context,
        List<short> installationIds,
        DateTime startDate,
        short numberOfDays,
        short numberOfForecastsInDay)
    {
      foreach (short installationId in installationIds)
      {
        context.AddForecastsToDatabase(
            installationId, startDate, numberOfDays, numberOfForecastsInDay);
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
        List<short> installationIds,
        DateTime startDate,
        short numberOfDays)
    {
      context.ForecastErrors.AddRange(GenerateTotalForecastErrors(
          installationIds, startDate, numberOfDays));

      context.SaveChanges();
    }

    public static void Clear(this AirlyContext context)
    {
      context.ArchiveMeasurements.RemoveRange(context.ArchiveMeasurements);
      context.ArchiveForecasts.RemoveRange(context.ArchiveForecasts);
      context.ForecastErrors.RemoveRange(context.ForecastErrors);
      context.SaveChanges();
    }
  }
}
