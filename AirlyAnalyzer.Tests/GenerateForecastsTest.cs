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
    private byte _numberOfForecasts;
    private short _numberOfDays = 25;
    private short _numberOfForecastsInDay = 23;

    [Fact]
    public void Return_valid_last_DateTime_when_one_day_forecasts()
    {
      // Arrange
      var endDate = _startDate.AddDays(1);
      _numberOfForecasts = 24;

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(_startDate, _numberOfForecasts, _requestMinutesOffset).ToList();

      // Assert
      Assert.Equal(endDate, forecasts.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_list_of_forecasts_with_specified_number_of_elements()
    {
      // Arrange
      _numberOfForecasts = 20;

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(_startDate, _numberOfForecasts, _requestMinutesOffset).ToList();

      // Assert
      Assert.Equal(_numberOfForecasts, forecasts.Count);
    }

    [Fact]
    public void Return_list_of_forecasts_from_many_days()
    {
      // Arrange 
      _numberOfDays = 25;
      _numberOfForecastsInDay = 23;

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(_startDate, _numberOfDays, _numberOfForecastsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(_numberOfDays * _numberOfForecastsInDay, forecasts.Count);
    }

    [Fact]
    public void Return_valid_last_forecast_date_when_many_days_forecasts()
    {
      // Arrange 
      _numberOfDays = 25;
      _numberOfForecastsInDay = 23;
      var endDate = _startDate.AddHours(_numberOfDays * _numberOfForecastsInDay);

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(_startDate, _numberOfDays, _numberOfForecastsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate, forecasts.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_valid_last_request_date_when_many_days_forecasts()
    {
      // Arrange 
      _numberOfDays = 25;
      _numberOfForecastsInDay = 23;
      var endRequestDate = _startDate.AddHours(_numberOfDays * _numberOfForecastsInDay)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var forecasts = AuxiliaryMethods
        .GenerateForecasts(_startDate, _numberOfDays, _numberOfForecastsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate, forecasts.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }
  }
}
