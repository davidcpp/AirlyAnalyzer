using System;
using System.Linq;
using AirlyAnalyzer.Tests.Models;
using Xunit;

namespace AirlyAnalyzer.Tests
{
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

      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(_installationId, _startDate, numberOfForecastErrors, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfForecastErrors, forecastErrors.Count);
    }

    [Fact]
    public void Return_correct_last_forecast_error_when_one_day()
    {
      // Arrange
      const int numberOfForecastErrors = 24;
      var endDate = _startDate.AddDays(1);

      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(_installationId, _startDate, numberOfForecastErrors, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecastErrors.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_list_of_forecast_errors_from_many_days()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfForecastErrorsInDay = 23;

      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(
          _installationId, _startDate, numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfDays * numberOfForecastErrorsInDay, forecastErrors.Count);
    }

    [Fact]
    public void Return_correct_last_forecast_error_date_when_many_days()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfForecastErrorsInDay = 23;
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

    [Fact]
    public void Return_correct_last_request_date_when_many_days()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfForecastErrorsInDay = 23;
      var endRequestDate = _startDate.AddDays(numberOfDays)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var forecastErrors = AuxiliaryMethods
        .GenerateForecastErrors(
          _installationId, _startDate, numberOfDays, numberOfForecastErrorsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate.ToLocalTime(), forecastErrors.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }
  }
}
