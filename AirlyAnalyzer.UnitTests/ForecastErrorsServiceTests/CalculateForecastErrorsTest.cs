namespace AirlyAnalyzer.UnitTests.ForecastErrorsServiceTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Calculation;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  [Collection("RepositoryTests")]
  public class CalculateForecastErrorsTest
  {
    private readonly IForecastErrorsCalculator _forecastErrorsCalculator;
    private readonly UnitOfWork _unitOfWork;
    private readonly AirlyContext _context;
    private readonly IConfiguration _config;

    private readonly DateTime _startDate;
    private readonly List<short> _installationIds;

    private readonly short _minNumberOfMeasurements;

    public CalculateForecastErrorsTest(RepositoryFixture fixture)
    {
      _config = fixture.Config;
      _unitOfWork = fixture.UnitOfWork;
      _context = fixture.Context;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      _minNumberOfMeasurements = fixture.Config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _forecastErrorsCalculator = new PlainForecastErrorsCalculator(_config);

      _context.Clear();
    }

    [Fact]
    public async Task returns_empty_new_forecast_error_list_when_no_installations()
    {
      // Arrange
      const short numberOfDays = 7;
      short numberOfElementsInDay = _minNumberOfMeasurements;

      var installationIds = new List<short>();

      _context.AddAllMeasurementsToDatabase(
          _installationIds,
          _startDate,
          numberOfDays,
          numberOfElementsInDay);

      _context.AddAllForecastsToDatabase(
          _installationIds,
          _startDate,
          numberOfDays,
          numberOfElementsInDay);

      var forecastErrorsService = new ForecastErrorsService(
          _unitOfWork,
          _forecastErrorsCalculator,
          installationIds,
          minNumberOfMeasurements: _minNumberOfMeasurements);

      // Act
      var (hourlyErrors, dailyErrors)
          = await forecastErrorsService.CalculateForecastErrors();

      // Assert
      Assert.Empty(hourlyErrors);
      Assert.Empty(dailyErrors);
    }

    [Fact]
    public async Task returns_empty_new_forecast_error_list_when_no_measurements_in_database()
    {
      // Arrange
      var forecastErrorsService = new ForecastErrorsService(
          _unitOfWork,
          _forecastErrorsCalculator,
          _installationIds);

      // Act
      var (hourlyErrors, dailyErrors)
          = await forecastErrorsService.CalculateForecastErrors();

      // Assert
      Assert.Empty(hourlyErrors);
      Assert.Empty(dailyErrors);
    }

    [Fact]
    public async Task returns_forecast_errors_with_correct_count()
    {
      // Arrange
      const short numberOfDays = 7;
      short numberOfElementsInDay = _minNumberOfMeasurements;

      int numberOfDailyErrors = numberOfDays * _installationIds.Count;
      int numberOfHourlyErrors = numberOfDailyErrors * numberOfElementsInDay;

      _context.AddAllMeasurementsToDatabase(
          _installationIds, _startDate, numberOfDays, numberOfElementsInDay);

      _context.AddAllForecastsToDatabase(
          _installationIds, _startDate, numberOfDays, numberOfElementsInDay);

      var forecastErrorsService = new ForecastErrorsService(
          _unitOfWork,
          _forecastErrorsCalculator,
          _installationIds,
          minNumberOfMeasurements: _minNumberOfMeasurements);

      // Act
      var (hourlyErrors, dailyErrors)
          = await forecastErrorsService.CalculateForecastErrors();

      // Assert
      Assert.Equal(numberOfHourlyErrors, hourlyErrors.Count);
      Assert.Equal(numberOfDailyErrors, dailyErrors.Count);
    }

    [Fact]
    public async Task returns_forecast_errors_with_correct_installaton_ids()
    {
      // Arrange
      const short numberOfDays = 7;
      short numberOfElementsInDay = _minNumberOfMeasurements;

      int numberOfDailyErrors = numberOfDays * _installationIds.Count;
      int numberOfHourlyErrors = numberOfDailyErrors * numberOfElementsInDay;

      _context.AddAllMeasurementsToDatabase(
          _installationIds, _startDate, numberOfDays, numberOfElementsInDay);

      _context.AddAllForecastsToDatabase(
          _installationIds, _startDate, numberOfDays, numberOfElementsInDay);

      var forecastErrorsService = new ForecastErrorsService(
          _unitOfWork,
          _forecastErrorsCalculator,
          _installationIds,
          minNumberOfMeasurements: _minNumberOfMeasurements);

      // Act
      var (hourlyErrors, dailyErrors)
          = await forecastErrorsService.CalculateForecastErrors();

      var hourlyErrorsInstallationIds
          = hourlyErrors.Select(fe => fe.InstallationId).Distinct().ToList();
      var dailyErrorsInstallationIds
          = dailyErrors.Select(fe => fe.InstallationId).Distinct().ToList();

      // Assert
      Assert.Equal(_installationIds.Count, hourlyErrorsInstallationIds.Count);
      Assert.Equal(_installationIds[0], hourlyErrorsInstallationIds[0]);
      Assert.Equal(_installationIds.Last(), hourlyErrorsInstallationIds.Last());

      Assert.Equal(_installationIds.Count, dailyErrorsInstallationIds.Count);
      Assert.Equal(_installationIds[0], dailyErrorsInstallationIds[0]);
      Assert.Equal(_installationIds.Last(), dailyErrorsInstallationIds.Last());
    }
  }
}
