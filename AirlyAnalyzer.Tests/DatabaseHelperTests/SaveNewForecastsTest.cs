namespace AirlyAnalyzer.Tests.DatabaseHelperTests
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

  [Collection("DatabaseHelperTests")]
  public class SaveNewForecastsTest : IDisposable
  {
    private readonly List<short> _installationIds;

    private readonly AirlyContext _context;
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
      Seed();
    }

    [Fact]
    public async Task do_not_save_forecasts_without_min_required_number()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short minNumberOfForecasts = 22;
      const short numberOfForecasts = 24;
      const int finalNumberOfForecasts = numberOfForecasts;
      const short hoursRequestInterval = 21;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddForecastsToDatabase(
          selectedInstallationId, numberOfForecasts, forecastsStartDate);

      var newForecasts = GenerateForecasts(
          selectedInstallationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      var databaseHelper = new DatabaseHelper(_context, minNumberOfForecasts);

      // Act
      await databaseHelper.SaveNewForecasts(selectedInstallationId, newForecasts);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_forecasts_with_min_required_number()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short minNumberOfForecasts = 22;
      const short numberOfForecasts = 24;
      const int finalNumberOfForecasts = minNumberOfForecasts + numberOfForecasts;
      const short hoursRequestInterval = minNumberOfForecasts;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddForecastsToDatabase(
          selectedInstallationId, numberOfForecasts, forecastsStartDate);

      var newForecasts = GenerateForecasts(
          selectedInstallationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      var databaseHelper = new DatabaseHelper(_context, minNumberOfForecasts);

      // Act
      await databaseHelper.SaveNewForecasts(selectedInstallationId, newForecasts);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_all_downloaded_forecasts_when_no_forecasts_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short minNumberOfForecasts = 22;
      const short numberOfForecasts = 24;
      const int finalNumberOfForecasts = numberOfForecasts;

      var newForecastsStartDate = _startDate;

      var newForecasts = GenerateForecasts(
          selectedInstallationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      var databaseHelper = new DatabaseHelper(_context, minNumberOfForecasts);

      // Act
      await databaseHelper.SaveNewForecasts(selectedInstallationId, newForecasts);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_forecasts_after_forecasts_from_several_installations()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short minNumberOfForecasts = 22;
      const short numberOfForecasts = 24;
      int finalNumberOfForecasts = 2 * numberOfForecasts * _installationIds.Count;
      const short hoursRequestInterval = numberOfForecasts;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddForecastsToDatabase(
          selectedInstallationId, numberOfForecasts, forecastsStartDate);

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        AddForecastsToDatabase(
            _installationIds[i], 2 * numberOfForecasts, forecastsStartDate);
      }

      var newForecasts = GenerateForecasts(
          selectedInstallationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      var databaseHelper = new DatabaseHelper(_context, minNumberOfForecasts);

      // Act
      await databaseHelper.SaveNewForecasts(selectedInstallationId, newForecasts);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    /* Private auxiliary methods */

    private void AddForecastsToDatabase(
        short selectedInstallationId, int numberOfForecasts, DateTime startDate)
    {
      _context.ArchiveForecasts.AddRange(
          GenerateForecasts(
              selectedInstallationId,
              startDate,
              numberOfForecasts));

      _context.SaveChanges();
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
