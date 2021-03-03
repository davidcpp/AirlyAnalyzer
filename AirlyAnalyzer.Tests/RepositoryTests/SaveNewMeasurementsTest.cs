﻿namespace AirlyAnalyzer.Tests.RepositoryTests
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
  public class SaveNewMeasurementsTest : IDisposable
  {
    private readonly AirlyContext _context;
    private readonly GenericRepository<AirQualityMeasurement> _measurementRepo;

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

      _measurementRepo = new GenericRepository<AirQualityMeasurement>(_context);

      Seed();
    }

    [Fact]
    public async Task save_measurements_with_min_required_number()
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
      await _measurementRepo.Add(newMeasurements);
      await _measurementRepo.SaveChangesAsync();

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_all_downloaded_measurements_when_no_measurements_in_database()
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
      await _measurementRepo.Add(newMeasurements);
      await _measurementRepo.SaveChangesAsync();

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task save_measurements_after_measurements_from_several_installations()
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
      await _measurementRepo.Add(newMeasurements);
      await _measurementRepo.SaveChangesAsync();

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
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
