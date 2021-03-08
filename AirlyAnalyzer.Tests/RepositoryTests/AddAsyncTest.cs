namespace AirlyAnalyzer.Tests.RepositoryTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AirlyAnalyzer.Data;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Xunit;
  using System.Threading.Tasks;
  using System.Linq;

  [Collection("RepositoryTests")]
  public class AddAsyncTest : IDisposable
  {
    private readonly AirlyContext _context;
    private readonly UnitOfWork _unitOfWork;

    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

    private readonly List<short> _installationIds;

    public AddAsyncTest()
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
    public async Task add_measurements_with_min_required_number()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short minNumberOfMeasurements = 22;
      const short numberOfMeasurements = 24;
      const int finalNumberOfMeasurements
          = numberOfMeasurements + minNumberOfMeasurements;
      const short hoursRequestInterval = minNumberOfMeasurements;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      _context.AddMeasurementsToDatabase(
          installationId, measurementsStartDate, numberOfMeasurements);

      var newMeasurements = GenerateMeasurements(
          installationId,
          newMeasurementsStartDate,
          numberOfMeasurements)
        .ToList();

      // Act
      await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task add_all_downloaded_measurements_when_no_measurements_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfMeasurements = 24;
      const int finalNumberOfMeasurements = numberOfMeasurements;

      var newMeasurementsStartDate = _startDate;

      var newMeasurements = GenerateMeasurements(
          installationId,
          newMeasurementsStartDate,
          numberOfMeasurements)
        .ToList();

      // Act
      await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task add_measurements_after_measurements_from_several_installations()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfMeasurements = 24;
      int finalNumberOfMeasurements
          = 2 * numberOfMeasurements * _installationIds.Count;
      const short hoursRequestInterval = numberOfMeasurements;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      _context.AddMeasurementsToDatabase(
          installationId, measurementsStartDate, numberOfMeasurements);

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        _context.AddMeasurementsToDatabase(
            _installationIds[i], measurementsStartDate, 2 * numberOfMeasurements);
      }

      var newMeasurements = GenerateMeasurements(
            installationId,
            newMeasurementsStartDate,
            numberOfMeasurements)
        .ToList();

      // Act
      await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task add_forecasts_with_min_required_number()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short minNumberOfForecasts = 22;
      const short numberOfForecasts = 24;
      const int finalNumberOfForecasts = numberOfForecasts + minNumberOfForecasts;
      const short hoursRequestInterval = minNumberOfForecasts;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      _context.AddForecastsToDatabase(
          installationId, forecastsStartDate, numberOfForecasts);

      var newForecasts = GenerateForecasts(
          installationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      // Act
      await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task add_all_downloaded_forecasts_when_no_forecasts_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfForecasts = 24;
      const int finalNumberOfForecasts = numberOfForecasts;

      var newForecastsStartDate = _startDate;

      var newForecasts = GenerateForecasts(
          installationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      // Act
      await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task add_forecasts_after_forecasts_from_several_installations()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfForecasts = 24;
      int finalNumberOfForecasts = 2 * numberOfForecasts * _installationIds.Count;
      const short hoursRequestInterval = numberOfForecasts;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      _context.AddForecastsToDatabase(
          installationId, forecastsStartDate, numberOfForecasts);

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        _context.AddForecastsToDatabase(
            _installationIds[i], forecastsStartDate, 2 * numberOfForecasts);
      }

      var newForecasts = GenerateForecasts(
          installationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      // Act
      await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
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
