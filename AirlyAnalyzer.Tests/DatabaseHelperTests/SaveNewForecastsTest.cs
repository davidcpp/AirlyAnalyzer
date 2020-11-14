namespace AirlyAnalyzer.Tests.DatabaseHelperTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Data;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Xunit;
  using System.Threading.Tasks;
  using System.Linq;

  [Collection("DatabaseHelperTests")]
  public class SaveNewForecastsTest : IDisposable
  {
    private const byte _requestMinutesOffset = 30;

    private readonly List<short> _installationIds;

    private readonly AirlyContext _testAirlyContext;
    private readonly DateTime _startDate;

    public SaveNewForecastsTest()
    {
      _startDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

      var inMemoryDatabaseOptions = new DbContextOptionsBuilder<AirlyContext>()
        .UseInMemoryDatabase("AirlyDatabase")
        .Options;

      string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

      var config = new ConfigurationBuilder()
        .AddJsonFile(configFilePath)
        .Build();

      _installationIds = config.GetSection("AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _testAirlyContext = new AirlyContext(inMemoryDatabaseOptions, config);
      Seed();
    }

    [Fact]
    public async Task do_not_save_forecasts_without_min_required_number()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfForecasts = 22;
      short numberOfForecasts = 24;
      int finalNumberOfForecasts = numberOfForecasts;
      short hoursRequestInterval = 21;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddForecastsToDatabase(selectedInstallationId, numberOfForecasts, forecastsStartDate);

      var newForecasts = GenerateForecasts(
          selectedInstallationId, newForecastsStartDate, numberOfForecasts, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfForecasts);

      // Act
      await databaseHelper.SaveNewForecasts(newForecasts, selectedInstallationId);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _testAirlyContext.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_forecasts_with_min_required_number()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfForecasts = 22;
      short numberOfForecasts = 24;
      int finalNumberOfForecasts = minNumberOfForecasts + numberOfForecasts;
      short hoursRequestInterval = minNumberOfForecasts;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddForecastsToDatabase(selectedInstallationId, numberOfForecasts, forecastsStartDate);

      var newForecasts = GenerateForecasts(
          selectedInstallationId, newForecastsStartDate, numberOfForecasts, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfForecasts);

      // Act
      await databaseHelper.SaveNewForecasts(newForecasts, selectedInstallationId);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _testAirlyContext.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_all_downloaded_forecasts_when_no_forecasts_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfForecasts = 22;
      short numberOfForecasts = 24;
      int finalNumberOfForecasts = numberOfForecasts;

      var newForecastsStartDate = _startDate;

      var newForecasts = GenerateForecasts(
          selectedInstallationId, newForecastsStartDate, numberOfForecasts, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfForecasts);

      // Act
      await databaseHelper.SaveNewForecasts(newForecasts, selectedInstallationId);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _testAirlyContext.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task save_forecasts_after_forecasts_from_several_installations()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfForecasts = 22;
      short numberOfForecasts = 24;
      int finalNumberOfForecasts = 2 * numberOfForecasts * _installationIds.Count;
      short hoursRequestInterval = numberOfForecasts;

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddForecastsToDatabase(selectedInstallationId, numberOfForecasts, forecastsStartDate);

      // all installations except the seleced
      for (int i = 1; i < _installationIds.Count; i++)
      {
        AddForecastsToDatabase(_installationIds[i], 2 * numberOfForecasts, forecastsStartDate);
      }

      var newForecasts = GenerateForecasts(
          selectedInstallationId, newForecastsStartDate, numberOfForecasts, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfForecasts);

      // Act
      await databaseHelper.SaveNewForecasts(newForecasts, selectedInstallationId);

      // Assert
      Assert.Equal(finalNumberOfForecasts, _testAirlyContext.ArchiveForecasts.Count());
    }

    /* Private auxiliary methods */

    private void AddForecastsToDatabase(short selectedInstallationId, int numberOfForecasts,
      DateTime startDate)
    {
      _testAirlyContext.ArchiveForecasts.AddRange(GenerateForecasts(
        selectedInstallationId, startDate, numberOfForecasts, _requestMinutesOffset));

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
    }
  }
}
