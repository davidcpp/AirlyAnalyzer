namespace AirlyAnalyzer.Tests.RepositoryTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  [Collection("RepositoryTests")]
  public class AddAsyncTest : IDisposable
  {
    private readonly AirlyContext _context;
    private readonly UnitOfWork _unitOfWork;

    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

    private readonly List<short> _installationIds;

    public AddAsyncTest()
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

      _unitOfWork = new UnitOfWork(_context);

      Seed(_context);
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

      for (int i = 0; i < _installationIds.Count; i++)
      {
        newMeasurements.AddRange(GenerateMeasurements(
            _installationIds[i], newMeasurementsStartDate, numberOfMeasurements));
      }

      // Act
      await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
    }

    [Fact]
    public async Task add_all_downloaded_measurements_when_no_measurements_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfMeasurements = 24;
      const int finalNumberOfMeasurements = numberOfMeasurements;

      var newMeasurementsStartDate = _startDate;

      var newMeasurements = GenerateMeasurements(
          installationId, newMeasurementsStartDate, numberOfMeasurements)
        .ToList();

      // Act
      await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(
          finalNumberOfMeasurements,
          _context.ArchiveMeasurements.Count());
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

      for (int i = 0; i < _installationIds.Count; i++)
      {
        newForecasts.AddRange(GenerateForecasts(
            _installationIds[i], newForecastsStartDate, numberOfForecasts));
      }

      // Act
      await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
    }

    [Fact]
    public async Task add_all_downloaded_forecasts_when_no_forecasts_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfForecasts = 24;
      const int finalNumberOfForecasts = numberOfForecasts;

      var newForecastsStartDate = _startDate;

      var newForecasts = GenerateForecasts(
          installationId, newForecastsStartDate, numberOfForecasts)
        .ToList();

      // Act
      await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecasts, _context.ArchiveForecasts.Count());
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

      for (int i = 0; i < _installationIds.Count; i++)
      {
        _context.AddHourlyForecastErrorsToDatabase(
            _installationIds[i], forecastErrorsStartDate, numberOfForecastErrors);
      }

      var newForecastErrors = new List<AirQualityForecastError>();

      for (int i = 0; i < _installationIds.Count; i++)
      {
        newForecastErrors.AddRange(
            GenerateHourlyForecastErrors(
                _installationIds[i],
                newForecastErrorsStartDate,
                numberOfForecastErrors));
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
      short installationId = _installationIds[0];
      const short numberOfForecastErrors = 24;
      const int finalNumberOfForecastErrors = numberOfForecastErrors;

      var newForecastErrorsStartDate = _startDate;

      var newForecastErrors = GenerateHourlyForecastErrors(
          installationId, newForecastErrorsStartDate, numberOfForecastErrors)
        .ToList();

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

      for (int i = 0; i < _installationIds.Count; i++)
      {
        _context.AddDailyForecastErrorsToDatabase(
            _installationIds[i], forecastErrorsStartDate, numberOfForecastErrors);
      }

      var newForecastErrors = new List<AirQualityForecastError>();

      for (int i = 0; i < _installationIds.Count; i++)
      {
        newForecastErrors.AddRange(
            GenerateDailyForecastErrors(
                _installationIds[i],
                newForecastErrorsStartDate,
                numberOfForecastErrors));
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
      short installationId = _installationIds[0];
      const short numberOfForecastErrors = 24;
      const int finalNumberOfForecastErrors = numberOfForecastErrors;

      var newForecastErrorsStartDate = _startDate;

      var newForecastErrors = GenerateDailyForecastErrors(
          installationId, newForecastErrorsStartDate, numberOfForecastErrors)
        .ToList();

      // Act
      await _unitOfWork.ForecastErrorRepository.AddAsync(newForecastErrors);
      await _unitOfWork.SaveChangesAsync();

      // Assert
      Assert.Equal(finalNumberOfForecastErrors, _context.ForecastErrors.Count());
    }

    public void Dispose()
    {
      _context.Dispose();
    }
  }
}
