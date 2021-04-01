namespace AirlyAnalyzer.Tests.RepositoryTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Tests.Fixtures;
  using AirlyAnalyzer.Tests.Helpers;
  using static AirlyAnalyzer.Tests.Helpers.ModelUtilities;
  using Xunit;

  [Collection("RepositoryTests")]
  public class AddAsyncTest
  {
    private readonly AirlyContext _context;
    private readonly UnitOfWork _unitOfWork;

    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

    private readonly List<short> _installationIds;

    public AddAsyncTest(RepositoryFixture fixture)
    {
      _context = fixture.Context;
      _unitOfWork = fixture.UnitOfWork;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      _context.Clear();
    }

    [Theory]
    [InlineData(22)]
    [InlineData(0)]
    public async Task do_not_add_repeated_measurements(
        short hoursRequestInterval)
    {
      // Arrange
      const short numberOfMeasurements = 24;
      int finalNumberOfMeasurements
          = _installationIds.Count * (numberOfMeasurements + hoursRequestInterval);

      var measurementsStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddHours(hoursRequestInterval);

      _context.AddAllMeasurementsToDatabase(
          _installationIds, measurementsStartDate, 1, numberOfMeasurements);

      var newMeasurements = new List<AirQualityMeasurement>();

      foreach (short installationId in _installationIds)
      {
        newMeasurements.AddRange(GenerateMeasurements(
            installationId, newMeasurementsStartDate, numberOfMeasurements));
      }

      // Act
      await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfMeasurements, _context.Measurements.Count());
    }

    [Fact]
    public async Task add_all_downloaded_measurements_when_no_measurements_in_database()
    {
      // Arrange
      const short numberOfMeasurements = 24;
      int finalNumberOfMeasurements = numberOfMeasurements * _installationIds.Count;
      var newMeasurementsStartDate = _startDate;

      var newMeasurements = new List<AirQualityMeasurement>();

      foreach (short installationId in _installationIds)
      {
        newMeasurements.AddRange(GenerateMeasurements(
            installationId, newMeasurementsStartDate, numberOfMeasurements));
      }

      // Act
      await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfMeasurements, _context.Measurements.Count());
    }

    [Theory]
    [InlineData(22)]
    [InlineData(0)]
    public async Task do_not_add_repeated_forecasts(
        short hoursRequestInterval)
    {
      // Arrange
      const short numberOfForecasts = 24;
      int finalNumberOfForecasts
          = _installationIds.Count * (numberOfForecasts + hoursRequestInterval);

      var forecastsStartDate = _startDate;
      var newForecastsStartDate = _startDate.AddHours(hoursRequestInterval);

      _context.AddAllForecastsToDatabase(
          _installationIds, forecastsStartDate, 1, numberOfForecasts);

      var newForecasts = new List<AirQualityForecast>();

      foreach (short installationId in _installationIds)
      {
        newForecasts.AddRange(GenerateForecasts(
            installationId, newForecastsStartDate, numberOfForecasts));
      }

      // Act
      await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.Forecasts.Count());
    }

    [Fact]
    public async Task add_all_downloaded_forecasts_when_no_forecasts_in_database()
    {
      // Arrange
      const short numberOfForecasts = 24;
      int finalNumberOfForecasts = numberOfForecasts * _installationIds.Count;
      var newForecastsStartDate = _startDate;

      var newForecasts = new List<AirQualityForecast>();

      foreach (short installationId in _installationIds)
      {
        newForecasts.AddRange(GenerateForecasts(
            installationId, newForecastsStartDate, numberOfForecasts));
      }

      // Act
      await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.Forecasts.Count());
    }

    [Theory]
    [InlineData(22)]
    [InlineData(0)]
    public async Task do_not_add_repeated_hourly_forecast_errors(
        short hoursRequestInterval)
    {
      // Arrange
      const short numberOfForecastErrors = 24;
      int finalNumberOfForecastErrors
          = _installationIds.Count * (numberOfForecastErrors + hoursRequestInterval);

      var forecastErrorsStartDate = _startDate;
      var newForecastErrorsStartDate = _startDate.AddHours(hoursRequestInterval);

      foreach (short installationId in _installationIds)
      {
        _context.AddHourlyForecastErrorsToDatabase(
            installationId, forecastErrorsStartDate, numberOfForecastErrors);
      }

      var newForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIds)
      {
        newForecastErrors.AddRange(GenerateHourlyForecastErrors(
            installationId, newForecastErrorsStartDate, numberOfForecastErrors));
      }

      // Act
      await _unitOfWork.ForecastErrorRepository.AddAsync(newForecastErrors);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecastErrors, _context.ForecastErrors.Count());
    }

    [Fact]
    public async Task add_hourly_forecast_errors_when_no_forecast_errors_in_database()
    {
      // Arrange
      const short numberOfForecastErrors = 24;
      int finalNumberOfForecastErrors
          = numberOfForecastErrors * _installationIds.Count;
      var newForecastErrorsStartDate = _startDate;

      var newForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIds)
      {
        newForecastErrors.AddRange(GenerateHourlyForecastErrors(
            installationId, newForecastErrorsStartDate, numberOfForecastErrors));
      }

      // Act
      await _unitOfWork.ForecastErrorRepository.AddAsync(newForecastErrors);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecastErrors, _context.ForecastErrors.Count());
    }

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    public async Task do_not_add_repeated_daily_forecast_errors(
        short daysRequestInterval)
    {
      // Arrange
      const short numberOfForecastErrors = 5;
      int finalNumberOfForecastErrors
          = _installationIds.Count * (numberOfForecastErrors + daysRequestInterval);

      var forecastErrorsStartDate = _startDate;
      var newForecastErrorsStartDate = _startDate.AddDays(daysRequestInterval);

      foreach (short installationId in _installationIds)
      {
        _context.AddDailyForecastErrorsToDatabase(
            installationId, forecastErrorsStartDate, numberOfForecastErrors);
      }

      var newForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIds)
      {
        newForecastErrors.AddRange(GenerateDailyForecastErrors(
            installationId, newForecastErrorsStartDate, numberOfForecastErrors));
      }

      // Act
      await _unitOfWork.ForecastErrorRepository.AddAsync(newForecastErrors);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecastErrors, _context.ForecastErrors.Count());
    }

    [Fact]
    public async Task add_daily_forecast_errors_when_no_forecast_errors_in_database()
    {
      // Arrange
      const short numberOfForecastErrors = 5;
      int finalNumberOfForecastErrors
          = numberOfForecastErrors * _installationIds.Count;
      var newForecastErrorsStartDate = _startDate;

      var newForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIds)
      {
        newForecastErrors.AddRange(GenerateDailyForecastErrors(
            installationId, newForecastErrorsStartDate, numberOfForecastErrors));
      }

      // Act
      await _unitOfWork.ForecastErrorRepository.AddAsync(newForecastErrors);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecastErrors, _context.ForecastErrors.Count());
    }
  }
}
