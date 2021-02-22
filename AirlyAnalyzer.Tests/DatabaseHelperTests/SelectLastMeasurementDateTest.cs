namespace AirlyAnalyzer.Tests.DatabaseHelperTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Data;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  [Collection("DatabaseHelperTests")]
  public class SelectLastMeasurementDateTest : IDisposable
  {
    private const byte _requestMinutesOffset = 30;
    private const short _minNumberOfMeasurements = 23;

    private readonly DatabaseHelper _databaseHelper;
    private readonly AirlyContext _context;

    private readonly DateTime _dateTimeMinValue;
    private readonly DateTime _startDate;

    private readonly List<short> _installationIds;

    public SelectLastMeasurementDateTest()
    {
      _dateTimeMinValue = new DateTime(2000, 1, 1);
      _startDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

      var inMemoryDatabaseOptions = new DbContextOptionsBuilder<AirlyContext>()
          .UseInMemoryDatabase("AirlyDatabase")
          .Options;

      string configFilePath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

      var config = new ConfigurationBuilder()
          .AddJsonFile(configFilePath)
          .Build();

      _installationIds = config.GetSection(
          "AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _context = new AirlyContext(inMemoryDatabaseOptions, config);
      _databaseHelper = new DatabaseHelper(_context, _minNumberOfMeasurements);
      Seed();
    }

    [Fact]
    public async Task date_time_min_value_when_no_data_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];

      // Act
      var lastMeasurementDate = await _databaseHelper
          .SelectLastMeasurementDate(selectedInstallationId);

      // Assert
      Assert.Equal(_dateTimeMinValue, lastMeasurementDate);
    }

    [Fact]
    public async Task date_time_min_value_when_only_data_from_other_installations_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short numberOfMeasurements = 23;
      var measurementsStartDate = _startDate;

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        AddMeasurementsToDatabase(
            _installationIds[i], measurementsStartDate, numberOfMeasurements);
      }

      // Act
      var lastMeasurementDate = await _databaseHelper
          .SelectLastMeasurementDate(selectedInstallationId);

      // Assert
      Assert.Equal(_dateTimeMinValue, lastMeasurementDate);
    }

    [Fact]
    public async Task proper_date_when_all_installations_data_in_database()
    {
      // Arrange
      short selectedInstallationId = _installationIds[0];
      short numberOfMeasurements = 22;
      short numberOfOtherMeasurements = 23;

      var measurementsStartDate = _startDate;
      var properDate = _startDate.AddHours(numberOfMeasurements);

      AddMeasurementsToDatabase(
          selectedInstallationId, measurementsStartDate, numberOfMeasurements);

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        AddMeasurementsToDatabase(
            _installationIds[i], measurementsStartDate, numberOfOtherMeasurements);
      }

      // Act
      var lastMeasurementDate = await _databaseHelper
          .SelectLastMeasurementDate(selectedInstallationId);

      // Assert
      Assert.Equal(properDate, lastMeasurementDate);
    }

    public void Dispose()
    {
      _context.Dispose();
    }

    /* Private auxiliary methods */

    private void AddMeasurementsToDatabase(short selectedInstallationId, DateTime startDate,
      int numberOfMeasurements)
    {
      _context.ArchiveMeasurements.AddRange(
          GenerateMeasurements(
              selectedInstallationId,
              startDate,
              numberOfMeasurements,
              _requestMinutesOffset));

      _context.SaveChanges();
    }

    private void Seed()
    {
      _context.Database.EnsureDeleted();
      _context.Database.EnsureCreated();
    }
  }
}
