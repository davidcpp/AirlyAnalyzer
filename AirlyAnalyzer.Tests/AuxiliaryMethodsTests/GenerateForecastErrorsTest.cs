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

    [Fact]
    public void Return_list_of_forecast_errors_from_one_day()
    {
      // Arrange
      const int numberOfForecastErrors = 20;
      const ForecastErrorType errorType = ForecastErrorType.Hourly;

      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(
          _installationId, errorType, _startDate, numberOfForecastErrors, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfForecastErrors, forecastErrors.Count);
    }

    [Theory]
    [InlineData(22)]
    [InlineData(24)]
    public void Return_correct_last_forecast_error_date_when_one_day(
      int numberOfForecastErrors)
    {
      // Arrange
      const ForecastErrorType errorType = ForecastErrorType.Hourly;
      var endDate = _startDate.AddHours(numberOfForecastErrors);

      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(
          _installationId, errorType, _startDate, numberOfForecastErrors, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecastErrors.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_list_of_forecast_errors_from_many_days()
    {
      // Arrange 
      const short numberOfDays = 15;
      const short numberOfForecastErrorsInDay = 23;

      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(
          _installationId, _startDate, numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfDays * numberOfForecastErrorsInDay, forecastErrors.Count);
    }

    [Theory]
    [InlineData(15, 22)]
    [InlineData(16, 22)]
    [InlineData(15, 24)]
    public void Return_correct_last_forecast_error_date_when_many_days(
      short numberOfDays, short numberOfForecastErrorsInDay)
    {
      // Arrange 
      var endDate = _startDate.AddDays(numberOfDays)
                              .AddHours(numberOfForecastErrorsInDay - 24);
      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(
          _installationId, _startDate, numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecastErrors.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Theory]
    [InlineData(15, 22, 22)]
    [InlineData(16, 22, 24)]
    [InlineData(15, 24, 24)]
    public void Return_correct_last_request_date_when_many_days(
      short numberOfDays, short numberOfForecastErrorsInDay, short lastDayRequestInterval)
    {
      // Arrange 
      var endRequestDate = _startDate.AddDays(numberOfDays)
                                     .AddHours(lastDayRequestInterval - 24)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(
          _installationId, _startDate, numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate.ToLocalTime(), forecastErrors.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }

    /* Tests for daily forecast errors */

    [Theory]
    [InlineData(22)]
    [InlineData(24)]
    public void Return_correct_list_of_daily_forecast_errors(short numberOfHourlyErrorsInDay)
    {
      // Arrange
      const int numberOfDailyErrors = 15;
      const ForecastErrorType errorType = ForecastErrorType.Daily;

      // Act
      var dailyErrors = AuxiliaryMethods.GenerateForecastErrors(_installationId, errorType,
        _startDate, numberOfDailyErrors, _requestMinutesOffset, numberOfHourlyErrorsInDay)
      .ToList();

      // Assert
      Assert.Equal(numberOfDailyErrors, dailyErrors.Count);
      Assert.Equal(_startDate.ToLocalTime(), dailyErrors[0].FromDateTime);
      Assert.Equal(_startDate.AddHours(numberOfHourlyErrorsInDay).ToLocalTime(), dailyErrors[0].TillDateTime);
      Assert.Equal(errorType, dailyErrors[0].ErrorType);
    }

    [Theory]
    [InlineData(15, 22)]
    [InlineData(16, 22)]
    [InlineData(15, 24)]
    public void Return_correct_last_daily_forecast_error_date(
      short numberOfDailyErrors, short numberOfHourlyErrorsInDay)
    {
      // Arrange
      const ForecastErrorType errorType = ForecastErrorType.Daily;
      var endDate = _startDate.AddDays(numberOfDailyErrors)
                              .AddHours(numberOfHourlyErrorsInDay - 24);

      // Act
      var dailyErrors = AuxiliaryMethods.GenerateForecastErrors(_installationId, errorType, _startDate,
        numberOfDailyErrors, _requestMinutesOffset, numberOfHourlyErrorsInDay)
      .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), dailyErrors.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Theory]
    [InlineData(15, 22, 22)]
    [InlineData(16, 22, 24)]
    [InlineData(15, 24, 24)]
    public void Return_correct_last_daily_forecast_error_request_date(
      short numberOfDailyErrors, short numberOfHourlyErrorsInDay, short lastDayRequestInterval)
    {
      // Arrange
      const ForecastErrorType errorType = ForecastErrorType.Daily;
      var endRequestDate = _startDate.AddDays(numberOfDailyErrors)
                              .AddHours(lastDayRequestInterval - 24)
                              .AddMinutes(_requestMinutesOffset);

      // Act
      var dailyErrors = AuxiliaryMethods.GenerateForecastErrors(_installationId, errorType, _startDate,
        numberOfDailyErrors, _requestMinutesOffset, numberOfHourlyErrorsInDay)
      .ToList();

      // Assert
      Assert.Equal(endRequestDate.ToLocalTime(), dailyErrors.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }
  }
}
