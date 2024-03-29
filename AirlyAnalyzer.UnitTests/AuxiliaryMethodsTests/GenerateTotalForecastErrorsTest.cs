﻿namespace AirlyAnalyzer.UnitTests.AuxiliaryMethodsTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Xunit;

  public class GenerateTotalForecastErrorsTest : IClassFixture<SimpleFixture>
  {
    private readonly List<short> _installationIds;
    private readonly DateTime _startDate;

    public GenerateTotalForecastErrorsTest(SimpleFixture fixture)
    {
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;
    }

    [Fact]
    public void correct_period_of_forecast_errors()
    {
      // Arrange
      const short numberOfDays = 10;

      // Act
      var forecastErrors = ModelUtilities.GenerateTotalForecastErrors(
          _installationIds, _startDate, numberOfDays)
        .ToList();

      // Assert
      Assert.Equal(ForecastErrorPeriod.Total, forecastErrors[0].Period);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void correct_number_of_forecast_errors(short numberOfDays)
    {
      // Act
      var forecastErrors = ModelUtilities.GenerateTotalForecastErrors(
          _installationIds, _startDate, numberOfDays)
        .ToList();

      // Assert
      Assert.Equal(_installationIds.Count + 1, forecastErrors.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public void correct_end_date_of_forecast_errors(short numberOfDays)
    {
      // Arrange
      var endDate = _startDate.AddDays(numberOfDays);

      // Act
      var forecastErrors = ModelUtilities.GenerateTotalForecastErrors(
          _installationIds, _startDate, numberOfDays)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecastErrors.Last().TillDateTime);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public void correct_last_request_date_of_forecast_errors(short numberOfDays)
    {
      // Arrange
      var endRequestDate = _startDate
          .AddDays(numberOfDays)
          .AddMinutes(ModelUtilities.RequestMinutesOffset);

      // Act
      var forecastErrors = ModelUtilities.GenerateTotalForecastErrors(
          _installationIds, _startDate, numberOfDays)
        .ToList();

      // Assert
      Assert.Equal(
          endRequestDate.ToLocalTime(),
          forecastErrors.Last().RequestDateTime);
    }
  }
}
