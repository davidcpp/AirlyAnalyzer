namespace AirlyAnalyzer.Tests.RepositoryTests
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

  [Collection("RepositoryTests")]
  public class SelectDataToProcessingTest : IDisposable
  {
    private const short _minNumberOfMeasurements = 23;

    private readonly AirlyContext _context;
    private readonly GenericRepository<AirQualityForecastError> _forecastErrorRepo;
    private readonly AirlyAnalyzerRepository _airlyAnalyzerRepo;

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

      _forecastErrorRepo
          = new GenericRepository<AirQualityForecastError>(_context);

      _airlyAnalyzerRepo = new AirlyAnalyzerRepository(
          _context,
          forecastErrorRepo: _forecastErrorRepo);

      Seed();
    }

    [Fact]
    public async Task empty_new_data_when_no_data_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) =
          await _airlyAnalyzerRepo.SelectDataToProcessing(installationId);

      // Assert
      Assert.Empty(newArchiveMeasurements);
      Assert.Empty(newArchiveForecasts);
    }

    [Fact]
    public async Task empty_new_data_when_no_data_to_process_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfProcessedDays = 5;
      const short numberOfElementsInDay = 24;

      AddElementsToDatabase(
          _startDate, numberOfProcessedDays, numberOfElementsInDay);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) =
          await _airlyAnalyzerRepo.SelectDataToProcessing(installationId);

      // Assert
      Assert.Empty(newArchiveMeasurements);
      Assert.Empty(newArchiveForecasts);
    }

    [Fact]
    public async Task new_data_when_only_data_to_process_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfNotProcessedDays = 1;
      const short numberOfNewMeasurementsInDay = 24;
      const short numberOfNewForecastsInDay = 24;

      var newMeasurementsStartDate = _startDate;
      var newForecastsStartDate = _startDate;

      AddNotProcessedDataToDatabase(
          newMeasurementsStartDate,
          newForecastsStartDate,
          numberOfNotProcessedDays,
          numberOfNewMeasurementsInDay,
          numberOfNewForecastsInDay);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) =
          await _airlyAnalyzerRepo.SelectDataToProcessing(installationId);

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
      short installationId = _installationIds[0];
      const short numberOfProcessedDays = 5;
      const short numberOfElementsInDay = 23;
      var processedDataStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddDays(numberOfProcessedDays);
      var newForecastsStartDate = _startDate.AddDays(numberOfProcessedDays);

      AddElementsToDatabase(
          processedDataStartDate, numberOfProcessedDays, numberOfElementsInDay);

      AddNotProcessedDataToDatabase(
          newMeasurementsStartDate,
          newForecastsStartDate,
          numberOfNotProcessedDays,
          numberOfNewMeasurementsInDay,
          numberOfNewForecastsInDay);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) =
          await _airlyAnalyzerRepo.SelectDataToProcessing(installationId);

      // Assert
      Assert.Equal(
          numberOfNewMeasurementsInDay * numberOfNotProcessedDays,
          newArchiveMeasurements.Count);
      Assert.Equal(
          numberOfNewForecastsInDay * numberOfNotProcessedDays,
          newArchiveForecasts.Count);
      Assert.Equal(installationId, newArchiveMeasurements[0].InstallationId);
      Assert.Equal(installationId, newArchiveForecasts[0].InstallationId);
    }

    /* Private auxiliary methods */
    private void AddElementsToDatabase(
        DateTime startDate, short numberOfDays, short numberOfElementsInDay)
    {
      var requestDate = startDate.AddDays(numberOfDays)
                                 .AddMinutes(RequestMinutesOffset);

      for (int i = 0; i < _installationIds.Count; i++)
      {
        short installationId = _installationIds[i];

        _context.AddMeasurementsToDatabase(
            installationId, startDate, numberOfDays, numberOfElementsInDay);

        _context.AddForecastsToDatabase(
            installationId, startDate, numberOfDays, numberOfElementsInDay);

        _context.ForecastErrors.AddRange(
            GenerateHourlyForecastErrors(
                installationId, startDate, numberOfDays, numberOfElementsInDay));

        _context.ForecastErrors.AddRange(
            GenerateDailyForecastErrors(
                installationId, startDate, numberOfDays, numberOfElementsInDay));

        int totalErrorDuration = ((numberOfDays - 1) * 24) + numberOfElementsInDay;

        _context.ForecastErrors.Add(
            CreateForecastError(
                installationId,
                ForecastErrorType.Total,
                startDate,
                requestDate,
                totalErrorDuration));
      }

      _context.SaveChanges();
    }

    // Method to adding new, not processed data for all installations 
    private void AddNotProcessedDataToDatabase(
        DateTime measurementsStartDate,
        DateTime forecastsStartDate,
        short numberOfNotProcessedDays,
        short numberOfMeasurementsInDay,
        short numberOfForecastsInDay)
    {
      for (int i = 0; i < _installationIds.Count; i++)
      {
        short installationId = _installationIds[i];

        _context.AddMeasurementsToDatabase(
            installationId,
            measurementsStartDate,
            numberOfNotProcessedDays,
            numberOfMeasurementsInDay);

        _context.AddForecastsToDatabase(
            installationId,
            forecastsStartDate,
            numberOfNotProcessedDays,
            numberOfForecastsInDay);
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
