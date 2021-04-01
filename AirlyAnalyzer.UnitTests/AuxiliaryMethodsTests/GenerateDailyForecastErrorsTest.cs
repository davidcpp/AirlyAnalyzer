namespace AirlyAnalyzer.UnitTests.AuxiliaryMethodsTests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Xunit;

  public class GenerateDailyForecastErrorsTest : IClassFixture<SimpleFixture>
  {
    private readonly short _installationId;
    private readonly DateTime _startDate;

    public GenerateDailyForecastErrorsTest(SimpleFixture fixture)
    {
      _startDate = fixture.StartDate;
      _installationId = fixture.InstallationId;
    }

    [Fact]
    public void correct_number_of_daily_forecast_errors()
    {
      // Arrange
      const int numberOfDailyErrors = 15;
      const short numberOfHourlyErrorsInDay = 24;

      // Act
      var dailyErrors = ModelUtilities.GenerateDailyForecastErrors(
          _installationId,
          _startDate,
          numberOfDailyErrors,
          numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(numberOfDailyErrors, dailyErrors.Count);
    }

    [Theory]
    [InlineData(22)]
    [InlineData(24)]
    public void correct_duration_time_of_daily_forecast_errors(
        short numberOfHourlyErrorsInDay)
    {
      // Arrange
      const int numberOfDailyErrors = 15;

      // Act
      var dailyErrors = ModelUtilities.GenerateDailyForecastErrors(
          _installationId,
          _startDate,
          numberOfDailyErrors,
          numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(_startDate.ToLocalTime(), dailyErrors[0].FromDateTime);
      Assert.Equal(
          _startDate.AddHours(numberOfHourlyErrorsInDay).ToLocalTime(),
          dailyErrors[0].TillDateTime);
    }

    [Fact]
    public void correct_error_type_of_daily_forecast_errors()
    {
      // Arrange
      const int numberOfDailyErrors = 15;
      const short numberOfHourlyErrorsInDay = 24;

      // Act
      var dailyErrors = ModelUtilities.GenerateDailyForecastErrors(
          _installationId,
          _startDate,
          numberOfDailyErrors,
          numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(ForecastErrorType.Daily, dailyErrors[0].ErrorType);
    }

    [Theory]
    [InlineData(15, 22)]
    [InlineData(16, 22)]
    [InlineData(15, 24)]
    public void correct_end_date_of_daily_forecast_errors(
        short numberOfDailyErrors, short numberOfHourlyErrorsInDay)
    {
      // Arrange
      var endDate = _startDate.AddDays(numberOfDailyErrors)
                              .AddHours(numberOfHourlyErrorsInDay - 24);

      // Act
      var dailyErrors = ModelUtilities.GenerateDailyForecastErrors(
          _installationId,
          _startDate,
          numberOfDailyErrors,
          numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), dailyErrors.Last().TillDateTime);
    }

    [Theory]
    [InlineData(15, 22, 22)]
    [InlineData(16, 22, 24)]
    [InlineData(15, 24, 24)]
    public void correct_last_request_date_of_daily_forecast_errors(
        short numberOfDailyErrors,
        short numberOfHourlyErrorsInDay,
        short lastDayRequestInterval)
    {
      // Arrange
      var endRequestDate = _startDate
          .AddDays(numberOfDailyErrors)
          .AddHours(lastDayRequestInterval - 24)
          .AddMinutes(ModelUtilities.RequestMinutesOffset);

      // Act
      var dailyErrors = ModelUtilities.GenerateDailyForecastErrors(
          _installationId,
          _startDate,
          numberOfDailyErrors,
          numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(
          endRequestDate.ToLocalTime(),
          dailyErrors.Last().RequestDateTime);
    }
  }
}
