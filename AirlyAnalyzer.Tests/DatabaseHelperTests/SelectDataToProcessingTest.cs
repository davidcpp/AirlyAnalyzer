namespace AirlyAnalyzer.Tests.DatabaseHelperTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  [Collection("DatabaseHelperTests")]
  public class SelectDataToProcessingTest : IDisposable
  {
    private const byte _requestMinutesOffset = 30;
    private const short _minNumberOfMeasurements = 23;

    private readonly DatabaseHelper _databaseHelper;
    private readonly AirlyContext _context;

    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

    private readonly List<short> _installationIds;

    public SelectDataToProcessingTest()
    {
      var inMemoryDatabaseOptions = new DbContextOptionsBuilder<AirlyContext>()
          .UseInMemoryDatabase("AirlyDatabase")
          .Options;

      string configFilePath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

      var config = new ConfigurationBuilder()
          .AddJsonFile(configFilePath)
          .Build();

      _installationIds = config
          .GetSection("AppSettings:AirlyApi:InstallationIds")
          .Get<List<short>>();

      _context = new AirlyContext(inMemoryDatabaseOptions, config);
      _databaseHelper = new DatabaseHelper(_context, _minNumberOfMeasurements);
      Seed();
    }

    [Fact]
    public async Task empty_new_data_when_no_data_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) =
          await _databaseHelper.SelectDataToProcessing(selectedInstallationId);

      // Assert
      Assert.Empty(newArchiveMeasurements);
      Assert.Empty(newArchiveForecasts);
    }

    [Fact]
    public async Task empty_new_data_when_no_data_to_process_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short numberOfProcessedDays = 5;
      const short numberOfElementsInDay = 24;

      AddElementsToDatabase(
          numberOfProcessedDays, numberOfElementsInDay, _startDate);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) =
          await _databaseHelper.SelectDataToProcessing(selectedInstallationId);

      // Assert
      Assert.Empty(newArchiveMeasurements);
      Assert.Empty(newArchiveForecasts);
    }

    [Fact]
    public async Task new_data_when_only_data_to_process_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short numberOfNotProcessedDays = 1;
      const short numberOfNewMeasurementsInDay = 24;
      const short numberOfNewForecastsInDay = 24;

      var newMeasurementsStartDate = _startDate;
      var newForecastsStartDate = _startDate;

      AddNotProcessedDataToDatabase(
          numberOfNotProcessedDays,
          numberOfNewMeasurementsInDay,
          numberOfNewForecastsInDay,
          newMeasurementsStartDate,
          newForecastsStartDate);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) =
          await _databaseHelper.SelectDataToProcessing(selectedInstallationId);

      // Assert
      Assert.Equal(
          numberOfNewMeasurementsInDay * numberOfNotProcessedDays,
          newArchiveMeasurements.Count);
      Assert.Equal(
          numberOfNewForecastsInDay * numberOfNotProcessedDays,
          newArchiveForecasts.Count);
    }

    [Theory]
    [InlineData(1, 2, 4)]
    [InlineData(2, 23, 24)]
    [InlineData(2, 24, 23)]
    public async Task new_data_when_data_to_process_from_several_installations_in_database(
        short numberOfNotProcessedDays,
        short numberOfNewMeasurementsInDay,
        short numberOfNewForecastsInDay)
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short numberOfProcessedDays = 5;
      const short numberOfElementsInDay = 23;
      var processedDataStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddDays(numberOfProcessedDays);
      var newForecastsStartDate = _startDate.AddDays(numberOfProcessedDays);

      AddElementsToDatabase(
          numberOfProcessedDays, numberOfElementsInDay, processedDataStartDate);

      AddNotProcessedDataToDatabase(
          numberOfNotProcessedDays,
          numberOfNewMeasurementsInDay,
          numberOfNewForecastsInDay,
          newMeasurementsStartDate,
          newForecastsStartDate);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) =
          await _databaseHelper.SelectDataToProcessing(selectedInstallationId);

      // Assert
      Assert.Equal(
          selectedInstallationId,
          newArchiveMeasurements[0].InstallationId);
      Assert.Equal(
          selectedInstallationId,
          newArchiveForecasts[0].InstallationId);
      Assert.Equal(
          numberOfNewMeasurementsInDay * numberOfNotProcessedDays,
          newArchiveMeasurements.Count);
      Assert.Equal(
          numberOfNewForecastsInDay * numberOfNotProcessedDays,
          newArchiveForecasts.Count);
    }

    /* Private auxiliary methods */
    private void AddElementsToDatabase(
        short numberOfDays, short numberOfElementsInDay, DateTime startDate)
    {
      var requestDate = startDate.AddDays(numberOfDays)
                                 .AddMinutes(_requestMinutesOffset);

      for (int i = 0; i < _installationIds.Count; i++)
      {
        short installationId = _installationIds[i];

        _context.ArchiveMeasurements.AddRange(
            GenerateMeasurements(
                installationId,
                startDate,
                numberOfDays,
                numberOfElementsInDay,
                _requestMinutesOffset));

        _context.ArchiveForecasts.AddRange(
            GenerateForecasts(
                installationId,
                startDate,
                numberOfDays,
                numberOfElementsInDay,
                _requestMinutesOffset));

        _context.ForecastErrors.AddRange(
            GenerateHourlyForecastErrors(
                installationId,
                startDate,
                numberOfDays,
                numberOfElementsInDay,
                _requestMinutesOffset));

        _context.ForecastErrors.AddRange(
            GenerateDailyForecastErrors(
                installationId,
                startDate,
                numberOfDays,
                _requestMinutesOffset,
                numberOfElementsInDay));

        int totalErrorDuration = ((numberOfDays - 1) * 24) + numberOfElementsInDay;

        _context.ForecastErrors.Add(
            CreateForecastError(
                installationId,
                ForecastErrorType.Total,
                startDate, requestDate,
                totalErrorDuration));
      }

      _context.SaveChanges();
    }

    private void AddNewMeasurementsToDatabase(
        short selectedInstallationId,
        short numberOfNotProcessedDays,
        short numberOfElementsInDay,
        DateTime startDate)
    {
      _context.ArchiveMeasurements.AddRange(
          GenerateMeasurements(
              selectedInstallationId,
              startDate,
              numberOfNotProcessedDays,
              numberOfElementsInDay,
              _requestMinutesOffset));

      _context.SaveChanges();
    }

    private void AddNewForecastsToDatabase(
        short selectedInstallationId,
        short numberOfNotProcessedDays,
        short numberOfElementsInDay,
        DateTime startDate)
    {
      _context.ArchiveForecasts.AddRange(
          GenerateForecasts(
              selectedInstallationId,
              startDate,
              numberOfNotProcessedDays,
              numberOfElementsInDay,
              _requestMinutesOffset));

      _context.SaveChanges();
    }

    // Method to adding new, not processed data for all installations 
    private void AddNotProcessedDataToDatabase(
        short numberOfNotProcessedDays,
        short numberOfMeasurementsInDay,
        short numberOfForecastsInDay,
        DateTime measurementsStartDate,
        DateTime forecastsStartDate)
    {
      for (int i = 0; i < _installationIds.Count; i++)
      {
        short installationId = _installationIds[i];

        AddNewMeasurementsToDatabase(
            installationId,
            numberOfNotProcessedDays,
            numberOfMeasurementsInDay,
            measurementsStartDate);

        AddNewForecastsToDatabase(
            installationId,
            numberOfNotProcessedDays,
            numberOfForecastsInDay,
            forecastsStartDate);
      }
    }

    private void Seed()
    {
      _context.Database.EnsureDeleted();
      _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
      _context.Dispose();
    }
  }
}
