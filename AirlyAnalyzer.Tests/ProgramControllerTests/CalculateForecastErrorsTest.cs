namespace AirlyAnalyzer.Tests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Calculation;
  using AirlyAnalyzer.Controllers;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Tests.Fixtures;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  [Collection("RepositoryTests")]
  public class CalculateForecastErrorsTest
  {
    private ForecastErrorsCalculator _forecastErrorsCalculation;
    private readonly UnitOfWork _unitOfWork;
    private readonly AirlyContext _context;

    private readonly DateTime _startDate;
    private List<short> _installationIds;

    private readonly short _minNumberOfMeasurements;

    public CalculateForecastErrorsTest(RepositoryFixture fixture)
    {
      _unitOfWork = fixture.UnitOfWork;
      _context = fixture.Context;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      _minNumberOfMeasurements = fixture.Config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _forecastErrorsCalculation =
          new ForecastErrorsCalculator(_minNumberOfMeasurements);

      Seed(_context);
    }

    [Fact]
    public async Task empty_new_forecast_errors_when_no_installations()
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

      var programController = new ProgramController(
          _unitOfWork, _forecastErrorsCalculation, installationIds);

      // Act
      var (hourlyErrors, dailyErrors)
          = await programController.CalculateForecastErrors();

      // Assert
      Assert.Empty(hourlyErrors);
      Assert.Empty(dailyErrors);
    }

    [Fact]
    public async Task empty_new_forecast_errors_when_no_measurements_in_database()
    {
      // Arrange
      var programController = new ProgramController(
          _unitOfWork, _forecastErrorsCalculation, _installationIds);

      // Act
      var (hourlyErrors, dailyErrors)
          = await programController.CalculateForecastErrors();

      // Assert
      Assert.Empty(hourlyErrors);
      Assert.Empty(dailyErrors);
    }

    [Fact]
    public async Task proper_number_of_calculated_forecast_errors()
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

      var programController = new ProgramController(
          _unitOfWork, _forecastErrorsCalculation, _installationIds);

      // Act
      var (hourlyErrors, dailyErrors)
          = await programController.CalculateForecastErrors();

      // Assert
      Assert.Equal(numberOfHourlyErrors, hourlyErrors.Count);
      Assert.Equal(numberOfDailyErrors, dailyErrors.Count);
    }
  }
}
