namespace AirlyAnalyzer.Tests.ForecastErrorCalculationTests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Models;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Xunit;

  public class CalculateDailyForecastErrorsTest
  {
    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

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
      const short installationId = 1;
      var startDate = _startDate;

      var newForecastErrors = GenerateHourlyForecastErrors(
          installationId,
          startDate,
          numberOfDays,
          numberOfForecastErrorsInDay)
        .ToList();

      var forecastErrorsCalculation
          = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var dailyForecastErrors = forecastErrorsCalculation
          .CalculateDailyForecastErrors(installationId, newForecastErrors);

      // Assert
      Assert.Equal(numberOfDailyForecastsErrors, dailyForecastErrors.Count);
    }

    [Fact]
    public void no_daily_forecast_error_when_number_of_elements_less_than_min()
    {
      // Arrange
      const short installationId = 1;
      const short minNumberOfMeasurements = 20;
      const short numberOfForecastErrors = 19;
      var startDate = _startDate;

      var newForecastErrors = GenerateHourlyForecastErrors(
          installationId, startDate, numberOfForecastErrors)
        .ToList();

      var forecastErrorsCalculation
          = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var dailyForecastErrors = forecastErrorsCalculation
          .CalculateDailyForecastErrors(installationId, newForecastErrors);

      // Assert
      Assert.Empty(dailyForecastErrors);
    }

    [Fact]
    public void correct_daily_forecast_error_when_number_of_elements_is_equal_min()
    {
      // Arrange
      const short installationId = 1;
      const short minNumberOfMeasurements = 20;
      const short numberOfForecastErrors = 20;
      var startDate = _startDate;
      var endDate = _startDate.AddHours(numberOfForecastErrors);

      var newForecastErrors = GenerateHourlyForecastErrors(
          installationId, startDate, numberOfForecastErrors)
        .ToList();

      var forecastErrorsCalculation
          = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var dailyForecastErrors = forecastErrorsCalculation
          .CalculateDailyForecastErrors(installationId, newForecastErrors);

      // Assert
      startDate = _startDate.ToLocalTime();
      endDate = endDate.ToLocalTime();

      Assert.Single(dailyForecastErrors);
      Assert.Equal(ForecastErrorType.Daily, dailyForecastErrors[0].ErrorType);
      Assert.Equal(installationId, dailyForecastErrors[0].InstallationId);
      Assert.Equal(startDate, dailyForecastErrors[0].FromDateTime);
      Assert.Equal(endDate, dailyForecastErrors[0].TillDateTime);
    }
  }
}
