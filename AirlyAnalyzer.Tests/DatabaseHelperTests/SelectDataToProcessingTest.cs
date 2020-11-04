namespace AirlyAnalyzer.Tests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Data;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Xunit;

  public class SelectDataToProcessingTest : IDisposable
  {
    private const byte _requestMinutesOffset = 30;
    private const short _minNumberOfMeasurements = 23;

    private readonly List<short> _installationIds;

    private readonly DatabaseHelper _databaseHelper;
    private readonly AirlyContext _testAirlyContext;
    private readonly DateTime _startDate;

    public static readonly ILoggerFactory _loggerFactory =
      LoggerFactory.Create(builder => builder.AddDebug());

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
      _databaseHelper = new DatabaseHelper(_testAirlyContext, _minNumberOfMeasurements);
      Seed();
    }

    [Fact]
    public void empty_new_data_when_all_previous_data_has_been_processed()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short numberOfProcessedDays = 5;
      const short numberOfElementsInDay = 24;
      var measurementsStartDate = _startDate;
      var forecastsStartDate = _startDate;
      var errorsStartDate = _startDate;

      AddElementsToDatabase(
        numberOfProcessedDays, numberOfElementsInDay, measurementsStartDate, forecastsStartDate, errorsStartDate);

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
    public void new_data_when_there_is_data_to_process(short numberOfNotProcessedDays, short numberOfNewElementsInDay)
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];

      const short numberOfProcessedDays = 5;

      const short numberOfElementsInDay = 23;

      var measurementsStartDate = _startDate;
      var forecastsStartDate = _startDate;
      var errorsStartDate = _startDate;

      AddElementsToDatabase(
        numberOfProcessedDays, numberOfElementsInDay, measurementsStartDate, forecastsStartDate, errorsStartDate);

      _testAirlyContext.ArchiveMeasurements.AddRange(
        GenerateMeasurements(selectedInstallationId, measurementsStartDate.AddDays(numberOfProcessedDays),
          numberOfNotProcessedDays, numberOfNewElementsInDay, _requestMinutesOffset));

      _testAirlyContext.ArchiveForecasts.AddRange(
        GenerateForecasts(selectedInstallationId, forecastsStartDate.AddDays(numberOfProcessedDays),
          numberOfNotProcessedDays, numberOfNewElementsInDay, _requestMinutesOffset));

      _testAirlyContext.SaveChanges();

      // Act
      _databaseHelper.SelectDataToProcessing(
        selectedInstallationId, out var newArchiveMeasurements, out var newArchiveForecasts);

      // Assert
      Assert.Equal(numberOfNewElementsInDay * numberOfNotProcessedDays, newArchiveMeasurements.Count);
      Assert.Equal(numberOfNewElementsInDay * numberOfNotProcessedDays, newArchiveForecasts.Count);
    }

    /* Private auxiliary methods */

    private void AddElementsToDatabase(short numberOfDays, short numberOfElementsInDay, DateTime measurementsStartDate,
      DateTime forecastsStartDate, DateTime errorsStartDate)
    {
      var requestDate = errorsStartDate.AddDays(numberOfDays)
                                       .AddMinutes(_requestMinutesOffset);

      for (int i = 0; i < _installationIds.Count; i++)
      {
        short installationId = _installationIds[i];

        _testAirlyContext.ArchiveMeasurements.AddRange(GenerateMeasurements(
          installationId, measurementsStartDate, numberOfDays, numberOfElementsInDay, _requestMinutesOffset));

        _testAirlyContext.ArchiveForecasts.AddRange(GenerateForecasts(
          installationId, forecastsStartDate, numberOfDays, numberOfElementsInDay, _requestMinutesOffset));

        _testAirlyContext.ForecastErrors.AddRange(GenerateForecastErrors(installationId,
          errorsStartDate, numberOfDays, numberOfElementsInDay, _requestMinutesOffset));

        _testAirlyContext.ForecastErrors.AddRange(GenerateForecastErrors(installationId, ForecastErrorType.Daily,
          errorsStartDate, numberOfDays, _requestMinutesOffset, numberOfElementsInDay));

        int totalErrorDuration = ((numberOfDays - 1) * 24) + numberOfElementsInDay;

        _testAirlyContext.ForecastErrors.Add(CreateForecastError(installationId, ForecastErrorType.Total,
          errorsStartDate, requestDate, totalErrorDuration));
      }

      _testAirlyContext.SaveChanges();
    }

    private void Seed()
    {
      _testAirlyContext.Database.EnsureDeleted();
      _testAirlyContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
      _testAirlyContext.Dispose();
      _loggerFactory.Dispose();
    }
  }
}
