namespace AirlyAnalyzer.UnitTests.ForecastErrorsCalculatorTests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Calculation;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using static AirlyAnalyzer.UnitTests.Helpers.ModelUtilities;
  using Xunit;

  public class CalculateDailyTest : IClassFixture<SimpleFixture>
  {
    private readonly short _installationId;

    private readonly DateTime _startDate;

    public CalculateDailyTest(SimpleFixture fixture)
    {
      _startDate = fixture.StartDate;
      _installationId = fixture.InstallationId;
    }

    [Theory]
    [InlineData(0, 0, 22, 0)]
    [InlineData(1, 21, 22, 0)]
    [InlineData(1, 22, 22, 1)]
    [InlineData(1, 23, 22, 1)]
    [InlineData(5, 21, 22, 0)]
    [InlineData(5, 22, 22, 5)]
    public void correct_number_of_daily_forecast_errors(
        short numberOfDays,
        short numberOfForecastErrorsInDay,
        short minNumberOfMeasurements,
        short numberOfDailyForecastsErrors)
    {
      // Arrange
      var startDate = _startDate;

      var newForecastErrors = GenerateHourlyForecastErrors(
          _installationId,
          startDate,
          numberOfDays,
          numberOfForecastErrorsInDay)
        .ToList();

      var forecastErrorsCalculator = new ForecastPlainErrorsCalculator();

      // Act
      var dailyForecastErrors = forecastErrorsCalculator.CalculateDaily(
          _installationId, minNumberOfMeasurements, newForecastErrors);

      // Assert
      Assert.Equal(numberOfDailyForecastsErrors, dailyForecastErrors.Count);
    }

    [Fact]
    public void returns_empty_daily_forecast_error_list_when_number_of_elements_less_than_min()
    {
      // Arrange
      const short minNumberOfMeasurements = 20;
      const short numberOfForecastErrors = 19;
      var startDate = _startDate;

      var newForecastErrors = GenerateHourlyForecastErrors(
          _installationId, startDate, numberOfForecastErrors)
        .ToList();

      var forecastErrorsCalculator = new ForecastPlainErrorsCalculator();

      // Act
      var dailyForecastErrors = forecastErrorsCalculator.CalculateDaily(
          _installationId, minNumberOfMeasurements, newForecastErrors);

      // Assert
      Assert.Empty(dailyForecastErrors);
    }

    [Fact]
    public void returns_correct_daily_forecast_error_when_number_of_elements_is_equal_min()
    {
      // Arrange
      const short minNumberOfMeasurements = 20;
      const short numberOfForecastErrors = 20;
      var startDate = _startDate;
      var endDate = _startDate.AddHours(numberOfForecastErrors);

      var newForecastErrors = GenerateHourlyForecastErrors(
          _installationId, startDate, numberOfForecastErrors)
        .ToList();

      var forecastErrorsCalculator = new ForecastPlainErrorsCalculator();

      // Act
      var dailyForecastErrors = forecastErrorsCalculator.CalculateDaily(
          _installationId, minNumberOfMeasurements, newForecastErrors);

      // Assert
      startDate = _startDate.ToLocalTime();
      endDate = endDate.ToLocalTime();

      Assert.Single(dailyForecastErrors);
      Assert.Equal(ForecastErrorPeriod.Day, dailyForecastErrors[0].Period);
      Assert.Equal(_installationId, dailyForecastErrors[0].InstallationId);
      Assert.Equal(startDate, dailyForecastErrors[0].FromDateTime);
      Assert.Equal(endDate, dailyForecastErrors[0].TillDateTime);
    }
  }
}
