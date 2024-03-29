﻿namespace AirlyAnalyzer.UnitTests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using static AirlyAnalyzer.UnitTests.Helpers.ModelUtilities;
  using Xunit;

  [Collection("RepositoryTests")]
  public class SaveAllAirQualityDataTest
  {
    private readonly ProgramController _programController;
    private readonly AirlyContext _context;
    private readonly DateTime _startDate;
    private readonly List<short> _installationIds;

    public SaveAllAirQualityDataTest(RepositoryFixture fixture)
    {
      _context = fixture.Context;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      _programController = new ProgramController(fixture.UnitOfWork);

      _context.Clear();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    public async Task returns_min_of_collections_count(
        short numberOfMeasurementDays, short numberOfForecastDays)
    {
      // Arrange
      const short numberOfElementsInDay = 24;

      int numberOfMeasurementElements
          = numberOfMeasurementDays * numberOfElementsInDay * _installationIds.Count;

      int numberOfForecastElements
          = numberOfForecastDays * numberOfElementsInDay * _installationIds.Count;

      var newMeasurements = new List<AirQualityMeasurement>();
      var newForecasts = new List<AirQualityForecast>();

      foreach (short installationId in _installationIds)
      {
        newMeasurements.AddRange(GenerateMeasurements(
            installationId,
            _startDate,
            numberOfMeasurementDays,
            numberOfElementsInDay));

        newForecasts.AddRange(GenerateForecasts(
            installationId,
            _startDate,
            numberOfForecastDays,
            numberOfElementsInDay));
      }

      // Act 
      int result = await _programController
          .SaveAllAirQualityData(newMeasurements, newForecasts);

      // Assert
      Assert.Equal(Math.Min(
          numberOfMeasurementElements, numberOfForecastElements), result);
    }

    [Fact]
    public async Task does_not_save_any_data_in_database_when_get_empty_new_ones()
    {
      // Arrange
      var newMeasurements = new List<AirQualityMeasurement>();
      var newForecasts = new List<AirQualityForecast>();

      // Act 
      await _programController
          .SaveAllAirQualityData(newMeasurements, newForecasts);

      // Assert
      Assert.Empty(_context.Measurements);
      Assert.Empty(_context.Forecasts);
    }

    [Fact]
    public async Task save_new_data_in_database_when_get_new_ones()
    {
      // Arrange
      const short numberOfMeasurementDays = 3;
      const short numberOfForecastDays = 7;
      const short numberOfElementsInDay = 24;

      int numberOfMeasurementElements
          = numberOfMeasurementDays * numberOfElementsInDay * _installationIds.Count;

      int numberOfForecastElements
          = numberOfForecastDays * numberOfElementsInDay * _installationIds.Count;

      var newMeasurements = new List<AirQualityMeasurement>();
      var newForecasts = new List<AirQualityForecast>();

      foreach (short installationId in _installationIds)
      {
        newMeasurements.AddRange(GenerateMeasurements(
            installationId,
            _startDate,
            numberOfMeasurementDays,
            numberOfElementsInDay));

        newForecasts.AddRange(GenerateForecasts(
            installationId,
            _startDate,
            numberOfForecastDays,
            numberOfElementsInDay));
      }

      // Act 
      await _programController
          .SaveAllAirQualityData(newMeasurements, newForecasts);

      // Assert
      Assert.Equal(numberOfMeasurementElements, _context.Measurements.Count());
      Assert.Equal(numberOfForecastElements, _context.Forecasts.Count());
    }
  }
}
