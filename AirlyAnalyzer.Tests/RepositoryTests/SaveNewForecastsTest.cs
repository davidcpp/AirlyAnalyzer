namespace AirlyAnalyzer.Tests.RepositoryTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Xunit;
  using System.Threading.Tasks;
  using System.Linq;

  [Collection("RepositoryTests")]
  public class SaveNewForecastsTest : IDisposable
  {
    private readonly List<short> _installationIds;

    private readonly AirlyContext _context;
    private readonly GenericRepository<AirQualityForecast> _forecastRepo;
    private readonly AirlyAnalyzerRepository _airlyAnalyzerRepo;

    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

    public SaveNewForecastsTest()
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

      _forecastRepo = new GenericRepository<AirQualityForecast>(_context);
      _airlyAnalyzerRepo = new AirlyAnalyzerRepository(
          _context, forecastRepo: _forecastRepo);

      Seed();
    }

    [Fact]
    public async Task do_not_save_forecasts_without_min_required_number()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short minNumberOfForecasts = 22;
      const short numberOfForecasts = 24;
      const int finalNumberOfForecasts = numberOfForecasts;
      const short hoursRequestInterval = 21;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      _context.AddForecastsToDatabase(
          installationId, forecastsStartDate, numberOfForecasts);

      var newForecasts = GenerateForecasts(
          installationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      // Act
      await _airlyAnalyzerRepo.SaveNewForecasts(
          installationId, minNumberOfForecasts, newForecasts);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_forecasts_with_min_required_number()
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
      await _airlyAnalyzerRepo.SaveNewForecasts(
          installationId, minNumberOfForecasts, newForecasts);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_all_downloaded_forecasts_when_no_forecasts_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short minNumberOfForecasts = 22;
      const short numberOfForecasts = 24;
      const int finalNumberOfForecasts = numberOfForecasts;

      var newForecastsStartDate = _startDate;

      var newForecasts = GenerateForecasts(
          installationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      // Act
      await _airlyAnalyzerRepo.SaveNewForecasts(
          installationId, minNumberOfForecasts, newForecasts);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_forecasts_after_forecasts_from_several_installations()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short minNumberOfForecasts = 22;
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
      await _airlyAnalyzerRepo.SaveNewForecasts(
          installationId, minNumberOfForecasts, newForecasts);

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
