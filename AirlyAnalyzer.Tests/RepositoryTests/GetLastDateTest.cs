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

  [Collection("RepositoryTests")]
  public class GetLastDateTest : IDisposable
  {
    private readonly AirlyContext _context;
    private readonly UnitOfWork _unitOfWork;

    private readonly DateTime _dateTimeMinValue = new DateTime(2000, 1, 1);
    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

    private readonly List<short> _installationIds;

    public GetLastDateTest()
    {
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

      _unitOfWork = new UnitOfWork(_context);

      Seed();
    }

    [Fact]
    public void date_time_min_value_when_no_data_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];

      // Act
      var lastMeasurementDate = _unitOfWork.MeasurementRepository
          .GetLastDate(installationId);

      // Assert
      Assert.Equal(_dateTimeMinValue, lastMeasurementDate);
    }

    [Fact]
    public void date_time_min_value_when_only_data_from_other_installations_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfMeasurements = 23;
      var measurementsStartDate = _startDate;

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        _context.AddMeasurementsToDatabase(
            _installationIds[i], measurementsStartDate, numberOfMeasurements);
      }

      // Act
      var lastMeasurementDate = _unitOfWork.MeasurementRepository
          .GetLastDate(installationId);

      // Assert
      Assert.Equal(_dateTimeMinValue, lastMeasurementDate);
    }

    [Fact]
    public void proper_date_when_all_installations_data_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfMeasurements = 22;
      const short numberOfOtherMeasurements = 23;

      var measurementsStartDate = _startDate;
      var properDate = _startDate.AddHours(numberOfMeasurements);

      _context.AddMeasurementsToDatabase(
          installationId, measurementsStartDate, numberOfMeasurements);

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        _context.AddMeasurementsToDatabase(
            _installationIds[i], measurementsStartDate, numberOfOtherMeasurements);
      }

      // Act
      var lastMeasurementDate = _unitOfWork.MeasurementRepository
          .GetLastDate(installationId);

      // Assert
      Assert.Equal(properDate, lastMeasurementDate);
    }

    public void Dispose()
    {
      _context.Dispose();
    }

    /* Private auxiliary methods */
    private void Seed()
    {
      _context.Database.EnsureDeleted();
      _context.Database.EnsureCreated();
    }
  }
}
