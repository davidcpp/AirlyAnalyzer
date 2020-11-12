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

  public class SaveNewMeasurementsTest : IDisposable
  {
    private const byte _requestMinutesOffset = 30;
    private const short _minNumberOfMeasurements = 23;

    private readonly List<short> _installationIds;

    private readonly AirlyContext _testAirlyContext;
    private readonly DateTime _startDate;

    public SaveNewMeasurementsTest()
    {
      _startDate = new DateTime(2001, 3, 15, 22, 0, 0, DateTimeKind.Utc);

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
    public async Task do_not_save_too_few_measurements()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfMeasurements = 22;
      short numberOfMeasurements = 24;
      int finalNumberOfMeasurements = numberOfMeasurements;
      short hoursRequestInterval = 21;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddMeasurementsToDatabase(selectedInstallationId, numberOfMeasurements, measurementsStartDate);

      var newMeasurements = GenerateMeasurements(
          selectedInstallationId, newMeasurementsStartDate, numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfMeasurements);

      // Assert
      Assert.Equal(finalNumberOfMeasurements, _testAirlyContext.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_measurements_with_min_required_number()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfMeasurements = 22;
      short numberOfMeasurements = 24;
      int finalNumberOfMeasurements = minNumberOfMeasurements + numberOfMeasurements;
      short hoursRequestInterval = minNumberOfMeasurements;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddMeasurementsToDatabase(selectedInstallationId, numberOfMeasurements, measurementsStartDate);

      var newMeasurements = GenerateMeasurements(
          selectedInstallationId, newMeasurementsStartDate, numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfMeasurements);

      // Assert
      Assert.Equal(finalNumberOfMeasurements, _testAirlyContext.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_all_downloaded_measurements()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfMeasurements = 22;
      short numberOfMeasurements = 24;
      int finalNumberOfMeasurements = 2 * numberOfMeasurements;
      short hoursRequestInterval = numberOfMeasurements;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddMeasurementsToDatabase(selectedInstallationId, numberOfMeasurements, measurementsStartDate);

      var newMeasurements = GenerateMeasurements(
          selectedInstallationId, newMeasurementsStartDate, numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfMeasurements);

      // Assert
      Assert.Equal(finalNumberOfMeasurements, _testAirlyContext.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_all_measurements_when_no_measurements_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfMeasurements = 22;
      short numberOfMeasurements = 24;
      int finalNumberOfMeasurements = numberOfMeasurements;

      var newMeasurementsStartDate = _startDate;

      var newMeasurements = GenerateMeasurements(
          selectedInstallationId, newMeasurementsStartDate, numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfMeasurements);

      // Assert
      Assert.Equal(finalNumberOfMeasurements, _testAirlyContext.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_measurements_when_there_are_measurements_for_other_installations_in_period()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short minNumberOfMeasurements = 22;
      short numberOfMeasurements = 24;
      int finalNumberOfMeasurements = 2 * numberOfMeasurements * _installationIds.Count;
      short hoursRequestInterval = numberOfMeasurements;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddMeasurementsToDatabase(selectedInstallationId, numberOfMeasurements, measurementsStartDate);

      // all installations except the seleced
      for (int i = 1; i < _installationIds.Count; i++)
      {
        AddMeasurementsToDatabase(_installationIds[i], 2 * numberOfMeasurements, measurementsStartDate);
      }

      var newMeasurements = GenerateMeasurements(
          selectedInstallationId, newMeasurementsStartDate, numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_testAirlyContext, minNumberOfMeasurements);

      // Assert
      Assert.Equal(finalNumberOfMeasurements, _testAirlyContext.ArchiveMeasurements.Count());
    }

    /* Private auxiliary methods */

    private void AddMeasurementsToDatabase(short selectedInstallationId, int numberOfMeasurements,
      DateTime startDate)
    {
      _testAirlyContext.ArchiveMeasurements.AddRange(GenerateMeasurements(
        selectedInstallationId, startDate, numberOfMeasurements, _requestMinutesOffset));

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
