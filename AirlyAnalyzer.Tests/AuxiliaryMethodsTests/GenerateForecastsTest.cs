namespace AirlyAnalyzer.Tests.AuxiliaryMethodsTests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Tests.Fixtures;
  using AirlyAnalyzer.Tests.Models;
  using Xunit;

  public class GenerateForecastsTest : IClassFixture<SimpleFixture>
  {
    private readonly short _installationId;
    private readonly DateTime _startDate;

    public GenerateForecastsTest(SimpleFixture fixture)
    {
      _startDate = fixture.StartDate;
      _installationId = fixture.InstallationId;
    }

    [Fact]
    public void returns_list_of_forecasts_from_one_day()
    {
      // Arrange
      const int numberOfForecasts = 20;

      // Act
      var forecasts = AuxiliaryMethods.GenerateForecasts(
          _installationId, _startDate, numberOfForecasts)
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
          _installationId, _startDate, numberOfForecasts)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecasts.Last().TillDateTime);
    }

    [Fact]
    public void returns_list_of_forecasts_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfForecastsInDay = 23;

      // Act
      var forecasts = AuxiliaryMethods.GenerateForecasts(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastsInDay)
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
          numberOfForecastsInDay)
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
      var endRequestDate = _startDate
          .AddDays(numberOfDays)
          .AddMinutes(AuxiliaryMethods.RequestMinutesOffset);

      // Act
      var forecasts = AuxiliaryMethods.GenerateForecasts(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastsInDay)
        .ToList();

      // Assert
      Assert.Equal(
          endRequestDate.ToLocalTime(),
          forecasts.Last().RequestDateTime);
    }
  }
}
