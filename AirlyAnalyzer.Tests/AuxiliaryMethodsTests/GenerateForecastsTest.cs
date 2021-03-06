﻿namespace AirlyAnalyzer.Tests.AuxiliaryMethodsTests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Tests.Models;
  using Xunit;

  public class GenerateForecastsTest
  {
    private const short _installationId = 1;
    private const byte _requestMinutesOffset = 30;

    private readonly DateTime _startDate
        = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void list_of_forecasts_from_one_day()
    {
      // Arrange
      const int numberOfForecasts = 20;

      // Act
      var forecasts = AuxiliaryMethods.GenerateForecasts(
          _installationId, _startDate, numberOfForecasts, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfForecasts, forecasts.Count);
    }

    [Fact]
    public void correct_end_date_of_forecasts_from_one_day()
    {
      // Arrange
      const int numberOfForecasts = 24;
      var endDate = _startDate.AddDays(1);

      // Act
      var forecasts = AuxiliaryMethods.GenerateForecasts(
          _installationId, _startDate, numberOfForecasts, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecasts.Last().TillDateTime);
    }

    [Fact]
    public void list_of_forecasts_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfForecastsInDay = 23;

      // Act
      var forecasts = AuxiliaryMethods.GenerateForecasts(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastsInDay,
          _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfDays * numberOfForecastsInDay, forecasts.Count);
    }

    [Fact]
    public void correct_end_date_of_forecasts_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfForecastsInDay = 23;
      var endDate = _startDate.AddDays(numberOfDays)
                              .AddHours(numberOfForecastsInDay - 24);

      // Act
      var forecasts = AuxiliaryMethods.GenerateForecasts(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastsInDay,
          _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecasts.Last().TillDateTime);
    }

    [Fact]
    public void correct_last_request_date_of_forecasts_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfForecastsInDay = 23;
      var endRequestDate = _startDate.AddDays(numberOfDays)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var forecasts = AuxiliaryMethods.GenerateForecasts(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastsInDay,
          _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(
          endRequestDate.ToLocalTime(),
          forecasts.Last().RequestDateTime);
    }
  }
}
