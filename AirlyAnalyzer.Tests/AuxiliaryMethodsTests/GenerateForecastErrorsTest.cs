namespace AirlyAnalyzer.Tests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Tests.Models;
  using Xunit;

  public class GenerateForecastErrorsTest
  {
    private readonly DateTime _startDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);
    private const byte _requestMinutesOffset = 30;
    private const short _installationId = 1;

    /* Tests for hourly forecast errors */

    [Fact]
    public void correct_number_of_forecast_errors_from_one_day()
    {
      // Arrange
      const int numberOfForecastErrors = 20;

      // Act
      var forecastErrors = AuxiliaryMethods.GenerateHourlyForecastErrors(
          _installationId, _startDate, numberOfForecastErrors, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfForecastErrors, forecastErrors.Count);
    }

    [Fact]
    public void correct_error_type_of_forecast_errors_from_one_day()
    {
      // Arrange
      const int numberOfForecastErrors = 20;

      // Act
      var forecastErrors = AuxiliaryMethods.GenerateHourlyForecastErrors(
          _installationId, _startDate, numberOfForecastErrors, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(ForecastErrorType.Hourly, forecastErrors[0].ErrorType);
    }

    [Theory]
    [InlineData(22)]
    [InlineData(24)]
    public void correct_end_date_of_forecast_errors_from_one_day(int numberOfForecastErrors)
    {
      // Arrange
      var endDate = _startDate.AddHours(numberOfForecastErrors);

      // Act
      var forecastErrors = AuxiliaryMethods.GenerateHourlyForecastErrors(
          _installationId, _startDate, numberOfForecastErrors, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecastErrors.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void correct_number_of_forecast_errors_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfForecastErrorsInDay = 23;

      // Act
      var forecastErrors = AuxiliaryMethods.GenerateHourlyForecastErrors(_installationId, _startDate,
          numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfDays * numberOfForecastErrorsInDay, forecastErrors.Count);
    }

    [Fact]
    public void correct_error_type_of_forecast_errors_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfForecastErrorsInDay = 23;

      // Act
      var forecastErrors = AuxiliaryMethods.GenerateHourlyForecastErrors(_installationId, _startDate,
          numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(ForecastErrorType.Hourly, forecastErrors[0].ErrorType);
    }

    [Theory]
    [InlineData(15, 22)]
    [InlineData(16, 22)]
    [InlineData(15, 24)]
    public void correct_end_date_of_forecast_errors_from_many_days(
      short numberOfDays, short numberOfForecastErrorsInDay)
    {
      // Arrange
      var endDate = _startDate.AddDays(numberOfDays)
                              .AddHours(numberOfForecastErrorsInDay - 24);
      // Act
      var forecastErrors = AuxiliaryMethods.GenerateHourlyForecastErrors(_installationId, _startDate,
          numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecastErrors.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Theory]
    [InlineData(15, 22, 22)]
    [InlineData(16, 22, 24)]
    [InlineData(15, 24, 24)]
    public void correct_last_request_date_of_forecast_errors_from_many_days(
      short numberOfDays, short numberOfForecastErrorsInDay, short lastDayRequestInterval)
    {
      // Arrange
      var endRequestDate = _startDate.AddDays(numberOfDays)
                                     .AddHours(lastDayRequestInterval - 24)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var forecastErrors = AuxiliaryMethods.GenerateHourlyForecastErrors(_installationId, _startDate,
          numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate.ToLocalTime(), forecastErrors.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }

    /* Tests for daily forecast errors */

    [Fact]
    public void correct_number_of_daily_forecast_errors()
    {
      // Arrange
      const int numberOfDailyErrors = 15;
      const short numberOfHourlyErrorsInDay = 24;

      // Act
      var dailyErrors = AuxiliaryMethods.GenerateDailyForecastErrors(_installationId, _startDate,
          numberOfDailyErrors, _requestMinutesOffset, numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(numberOfDailyErrors, dailyErrors.Count);
    }

    [Theory]
    [InlineData(22)]
    [InlineData(24)]
    public void correct_duration_time_of_daily_forecast_errors(short numberOfHourlyErrorsInDay)
    {
      // Arrange
      const int numberOfDailyErrors = 15;

      // Act
      var dailyErrors = AuxiliaryMethods.GenerateDailyForecastErrors(_installationId, _startDate,
          numberOfDailyErrors, _requestMinutesOffset, numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(_startDate.ToLocalTime(), dailyErrors[0].FromDateTime);
      Assert.Equal(_startDate.AddHours(numberOfHourlyErrorsInDay).ToLocalTime(), dailyErrors[0].TillDateTime);
    }

    [Fact]
    public void correct_error_type_of_daily_forecast_errors()
    {
      // Arrange
      const int numberOfDailyErrors = 15;
      const short numberOfHourlyErrorsInDay = 24;

      // Act
      var dailyErrors = AuxiliaryMethods.GenerateDailyForecastErrors(_installationId, _startDate,
          numberOfDailyErrors, _requestMinutesOffset, numberOfHourlyErrorsInDay)
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
      var dailyErrors = AuxiliaryMethods.GenerateDailyForecastErrors(_installationId, _startDate,
          numberOfDailyErrors, _requestMinutesOffset, numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), dailyErrors.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Theory]
    [InlineData(15, 22, 22)]
    [InlineData(16, 22, 24)]
    [InlineData(15, 24, 24)]
    public void correct_last_request_date_of_daily_forecast_errors(
      short numberOfDailyErrors, short numberOfHourlyErrorsInDay, short lastDayRequestInterval)
    {
      // Arrange
      var endRequestDate = _startDate.AddDays(numberOfDailyErrors)
                                     .AddHours(lastDayRequestInterval - 24)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var dailyErrors = AuxiliaryMethods.GenerateDailyForecastErrors(_installationId,_startDate,
          numberOfDailyErrors, _requestMinutesOffset, numberOfHourlyErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate.ToLocalTime(), dailyErrors.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }
  }
}
