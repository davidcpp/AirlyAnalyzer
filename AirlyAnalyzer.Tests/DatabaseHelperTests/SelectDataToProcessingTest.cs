namespace AirlyAnalyzer.Tests
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Data;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Logging.Debug;
  using Xunit;
  using System.IO;

  public class SelectDataToProcessingTest : IDisposable
  {
    private const byte _requestMinutesOffset = 30;
    private const short _minNumberOfMeasurements = 23;

    private List<short> _installationIds;

    private DatabaseHelper _databaseHelper;
    private AirlyContext _testAirlyContext;
    private readonly DateTime _startDate;

    public static readonly LoggerFactory _loggerFactory =
       new LoggerFactory(new[] { new DebugLoggerProvider() });

    public SelectDataToProcessingTest()
    {
      _startDate = new DateTime(2001, 3, 15, 22, 0, 0, DateTimeKind.Utc);

      var inMemoryDatabaseOptions = new DbContextOptionsBuilder<AirlyContext>()
        .UseInMemoryDatabase("AirlyDatabase")
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .UseLoggerFactory(_loggerFactory)
        .Options;

      string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

      var config = new ConfigurationBuilder()
        .AddJsonFile(configFilePath)
        .Build();

      _installationIds = config.GetSection("AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _testAirlyContext = new AirlyContext(inMemoryDatabaseOptions, config);
      Seed();
      _databaseHelper = new DatabaseHelper(_testAirlyContext, _minNumberOfMeasurements);
    }

    //public void Dispose

    [Fact]
    public void empty_new_data_when_all_previous_data_has_been_processed()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short numberOfDays = 5;
      const short numberOfMeasurementsInDay = 24;
      const short numberOfForecastsInDay = 24;
      const short numberOfErrorsInDay = 24;
      var measurementsStartDate = _startDate;
      var forecastsStartDate = _startDate;
      var errorsStartDate = _startDate;

      AddElementsToDatabase(numberOfDays, numberOfMeasurementsInDay, numberOfForecastsInDay, numberOfErrorsInDay,
        measurementsStartDate, forecastsStartDate, errorsStartDate);

      // Act
      _databaseHelper.SelectDataToProcessing(
        selectedInstallationId, out var newArchiveMeasurements, out var newArchiveForecasts);

      // Assert
      Assert.Empty(newArchiveMeasurements);
      Assert.Empty(newArchiveForecasts);
    }

    [Fact]
    public void empty_new_data_when_there_is_no_data()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];

      // Act
      _databaseHelper.SelectDataToProcessing(
        selectedInstallationId, out var newArchiveMeasurements, out var newArchiveForecasts);

      // Assert
      Assert.Empty(newArchiveMeasurements);
      Assert.Empty(newArchiveForecasts);
    }

    [Theory]
    [InlineData(1, 24)]
    [InlineData(2, 24)]
    [InlineData(2, 23)]
    public void new_data_when_there_is_data_to_process(short numberOfNotProcessedDays, short numberOfErrorsInDay)
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];

      const short numberOfDays = 5;

      const short numberOfMeasurementsInDay = 24;
      const short numberOfForecastsInDay = 24;

      var measurementsStartDate = _startDate;
      var forecastsStartDate = _startDate;
      var errorsStartDate = _startDate;

      AddElementsToDatabase(numberOfDays, numberOfMeasurementsInDay, numberOfForecastsInDay, numberOfErrorsInDay,
        measurementsStartDate, forecastsStartDate, errorsStartDate);

      _testAirlyContext.ArchiveMeasurements.AddRange(GenerateMeasurements(selectedInstallationId,
        measurementsStartDate.AddDays(numberOfDays), numberOfNotProcessedDays, numberOfMeasurementsInDay, _requestMinutesOffset));

      _testAirlyContext.ArchiveForecasts.AddRange(GenerateForecasts(selectedInstallationId,
        forecastsStartDate.AddDays(numberOfDays), numberOfNotProcessedDays, numberOfForecastsInDay, _requestMinutesOffset));

      _testAirlyContext.SaveChanges();

      // Act
      _databaseHelper.SelectDataToProcessing(
        selectedInstallationId, out var newArchiveMeasurements, out var newArchiveForecasts);

      // Assert
      Assert.Equal(numberOfMeasurementsInDay * numberOfNotProcessedDays, newArchiveMeasurements.Count);
      Assert.Equal(numberOfForecastsInDay * numberOfNotProcessedDays, newArchiveForecasts.Count);
    }

    /* Private auxiliary methods */

    private void AddElementsToDatabase(short numberOfDays, short numberOfMeasurementsInDay, short numberOfForecastsInDay,
      short numberOfErrorsInDay, DateTime measurementsStartDate, DateTime forecastsStartDate, DateTime errorsStartDate)
    {
      var requestDate = errorsStartDate.AddDays(numberOfDays)
                                       .AddMinutes(_requestMinutesOffset);

      var forecastErrors = new List<AirQualityForecastError>();

      for (int i = 0; i < _installationIds.Count; i++)
      {
        short installationId = _installationIds[i];

        _testAirlyContext.ArchiveMeasurements.AddRange(GenerateMeasurements(
          installationId, measurementsStartDate, numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset));

        _testAirlyContext.ArchiveForecasts.AddRange(GenerateForecasts(
          installationId, forecastsStartDate, numberOfDays, numberOfForecastsInDay, _requestMinutesOffset));

        forecastErrors.AddRange(GenerateForecastErrors(installationId, errorsStartDate,
          numberOfDays, numberOfErrorsInDay, _requestMinutesOffset));

        forecastErrors.AddRange(GenerateForecastErrors(installationId, ForecastErrorType.Daily, errorsStartDate,
          numberOfDays, _requestMinutesOffset, numberOfErrorsInDay));

        int totalErrorDuration = ((numberOfDays - 1) * 24) + numberOfErrorsInDay;

        forecastErrors.Add(CreateForecastError(installationId, ForecastErrorType.Total, errorsStartDate,
          requestDate, totalErrorDuration));
      }

      _testAirlyContext.ForecastErrors.AddRange(forecastErrors);
      _testAirlyContext.SaveChanges();
    }

    private void Seed()
    {
      _testAirlyContext.Database.EnsureDeleted();
      _testAirlyContext.Database.EnsureCreated();
      _testAirlyContext.SaveChanges();
    }

    public void Dispose()
    {
      _testAirlyContext.Dispose();
      _loggerFactory.Dispose();
    }
  }
}
