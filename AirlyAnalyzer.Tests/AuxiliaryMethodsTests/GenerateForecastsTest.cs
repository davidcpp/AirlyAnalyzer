using System;
using System.Linq;
using AirlyAnalyzer.Tests.Models;
using Xunit;

namespace AirlyAnalyzer.Tests
{
  public class GenerateForecastsTest
  {
    private readonly DateTime _startDate = new DateTime(2001, 3, 15, 22, 0, 0, DateTimeKind.Local);
    private const byte _requestMinutesOffset = 30;
    private const short _installationId = 1;

    [Fact]
    public void Return_list_of_forecasts_from_one_day()
    {
      // Arrange
      const int numberOfForecasts = 20;

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(_installationId, _startDate, numberOfForecasts, _requestMinutesOffset).ToList();

      // Assert
      Assert.Equal(numberOfForecasts, forecasts.Count);
    }

    [Fact]
    public void Return_correct_last_DateTime_of_forecasts_from_one_day()
    {
      // Arrange
      const int numberOfForecasts = 24;
      var endDate = _startDate.AddDays(1);

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(_installationId, _startDate, numberOfForecasts, _requestMinutesOffset).ToList();

      // Assert
      Assert.Equal(endDate, forecasts.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_list_of_forecasts_from_many_days()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfForecastsInDay = 23;

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(
          _installationId, _startDate, numberOfDays, numberOfForecastsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfDays * numberOfForecastsInDay, forecasts.Count);
    }

    [Fact]
    public void Return_correct_last_forecast_date_when_many_days_forecasts()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfForecastsInDay = 23;
      var endDate = _startDate.AddHours(numberOfDays * numberOfForecastsInDay);

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(
          _installationId, _startDate, numberOfDays, numberOfForecastsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate, forecasts.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_correct_last_request_date_when_many_days_forecasts()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfForecastsInDay = 23;
      var endRequestDate = _startDate.AddHours(numberOfDays * numberOfForecastsInDay)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(
          _installationId, _startDate, numberOfDays, numberOfForecastsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate, forecasts.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }
  }
}
