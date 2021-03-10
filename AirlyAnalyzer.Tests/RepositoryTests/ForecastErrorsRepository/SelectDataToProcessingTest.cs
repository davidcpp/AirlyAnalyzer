namespace AirlyAnalyzer.Tests.RepositoryTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  [Collection("RepositoryTests")]
  public class SelectDataToProcessingTest : IDisposable
  {
    private readonly AirlyContext _context;
    private readonly UnitOfWork _unitOfWork;

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

      _unitOfWork = new UnitOfWork(_context);

      Seed();
    }

    [Fact]
    public async Task empty_new_data_when_no_data_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(installationId);

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

      _context.AddElementsToDatabase(
          _installationIds,
          _startDate,
          numberOfProcessedDays,
          numberOfElementsInDay);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(installationId);

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

      _context.AddNotProcessedDataToDatabase(
          _installationIds,
          newMeasurementsStartDate,
          newForecastsStartDate,
          numberOfNotProcessedDays,
          numberOfNewMeasurementsInDay,
          numberOfNewForecastsInDay);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(installationId);

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

      _context.AddElementsToDatabase(
          _installationIds,
          processedDataStartDate,
          numberOfProcessedDays,
          numberOfElementsInDay);

      _context.AddNotProcessedDataToDatabase(
          _installationIds,
          newMeasurementsStartDate,
          newForecastsStartDate,
          numberOfNotProcessedDays,
          numberOfNewMeasurementsInDay,
          numberOfNewForecastsInDay);

      // Act
      var (newArchiveMeasurements, newArchiveForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(installationId);

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
