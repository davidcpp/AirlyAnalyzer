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
  public class SaveNewMeasurementsTest : IDisposable
  {
    private const byte _requestMinutesOffset = 30;

    private readonly AirlyContext _context;
    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

    private readonly List<short> _installationIds;

    public SaveNewMeasurementsTest()
    {
      var inMemoryDatabaseOptions = new DbContextOptionsBuilder<AirlyContext>()
          .UseInMemoryDatabase("AirlyDatabase")
          .Options;

      string configFilePath
          = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

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
    public async Task do_not_save_measurements_without_min_required_number()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short minNumberOfMeasurements = 22;
      const short numberOfMeasurements = 24;
      const int finalNumberOfMeasurements = numberOfMeasurements;
      const short hoursRequestInterval = 21;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddMeasurementsToDatabase(
          selectedInstallationId, numberOfMeasurements, measurementsStartDate);

      var newMeasurements = GenerateMeasurements(
          selectedInstallationId,
          newMeasurementsStartDate,
          numberOfMeasurements,
          _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_context, minNumberOfMeasurements);

      // Act
      await databaseHelper
          .SaveNewMeasurements(selectedInstallationId, newMeasurements);

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_measurements_with_min_required_number()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short minNumberOfMeasurements = 22;
      const short numberOfMeasurements = 24;
      const int finalNumberOfMeasurements
          = minNumberOfMeasurements + numberOfMeasurements;
      const short hoursRequestInterval = minNumberOfMeasurements;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddMeasurementsToDatabase(
          selectedInstallationId, numberOfMeasurements, measurementsStartDate);

      var newMeasurements = GenerateMeasurements(
          selectedInstallationId,
          newMeasurementsStartDate,
          numberOfMeasurements,
          _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_context, minNumberOfMeasurements);

      // Act
      await databaseHelper
          .SaveNewMeasurements(selectedInstallationId, newMeasurements);

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_all_downloaded_measurements_when_no_measurements_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short minNumberOfMeasurements = 22;
      const short numberOfMeasurements = 24;
      const int finalNumberOfMeasurements = numberOfMeasurements;

      var newMeasurementsStartDate = _startDate;

      var newMeasurements = GenerateMeasurements(
          selectedInstallationId,
          newMeasurementsStartDate,
          numberOfMeasurements,
          _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_context, minNumberOfMeasurements);

      // Act
      await databaseHelper
          .SaveNewMeasurements(selectedInstallationId, newMeasurements);

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_measurements_after_measurements_from_several_installations()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      const short minNumberOfMeasurements = 22;
      const short numberOfMeasurements = 24;
      int finalNumberOfMeasurements
          = 2 * numberOfMeasurements * _installationIds.Count;
      const short hoursRequestInterval = numberOfMeasurements;

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      AddMeasurementsToDatabase(
          selectedInstallationId, numberOfMeasurements, measurementsStartDate);

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        AddMeasurementsToDatabase(
            _installationIds[i], 2 * numberOfMeasurements, measurementsStartDate);
      }

      var newMeasurements = GenerateMeasurements(
            selectedInstallationId,
            newMeasurementsStartDate,
            numberOfMeasurements,
            _requestMinutesOffset)
        .ToList();

      var databaseHelper = new DatabaseHelper(_context, minNumberOfMeasurements);

      // Act
      await databaseHelper
          .SaveNewMeasurements(selectedInstallationId, newMeasurements);

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    /* Private auxiliary methods */

    private void AddMeasurementsToDatabase(
        short selectedInstallationId, int numberOfMeasurements, DateTime startDate)
    {
      _context.ArchiveMeasurements.AddRange(
          GenerateMeasurements(
              selectedInstallationId,
              startDate, numberOfMeasurements,
              _requestMinutesOffset));

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
