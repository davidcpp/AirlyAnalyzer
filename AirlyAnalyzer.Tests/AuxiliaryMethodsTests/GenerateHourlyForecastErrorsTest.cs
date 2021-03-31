namespace AirlyAnalyzer.Tests.AuxiliaryMethodsTests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Tests.Fixtures;
  using AirlyAnalyzer.Tests.Models;
  using Xunit;

  public class GenerateHourlyForecastErrorsTest : IClassFixture<SimpleFixture>
  {
    private readonly short _installationId;
    private readonly DateTime _startDate;

    public GenerateHourlyForecastErrorsTest(SimpleFixture fixture)
    {
      _startDate = fixture.StartDate;
      _installationId = fixture.InstallationId;
    }

    [Fact]
    public void correct_number_of_forecast_errors_from_one_day()
    {
      // Arrange
      const int numberOfForecastErrors = 20;

      // Act
      var forecastErrors = ModelUtilities.GenerateHourlyForecastErrors(
          _installationId,
          _startDate,
          numberOfForecastErrors)
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
      var forecastErrors = ModelUtilities.GenerateHourlyForecastErrors(
          _installationId,
          _startDate,
          numberOfForecastErrors)
        .ToList();

      // Assert
      Assert.Equal(ForecastErrorType.Hourly, forecastErrors[0].ErrorType);
    }

    [Theory]
    [InlineData(22)]
    [InlineData(24)]
    public void correct_end_date_of_forecast_errors_from_one_day(
        int numberOfForecastErrors)
    {
      // Arrange
      var endDate = _startDate.AddHours(numberOfForecastErrors);

      // Act
      var forecastErrors = ModelUtilities.GenerateHourlyForecastErrors(
          _installationId,
          _startDate,
          numberOfForecastErrors)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecastErrors.Last().TillDateTime);
    }

    [Fact]
    public void correct_number_of_forecast_errors_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfForecastErrorsInDay = 23;

      // Act
      var forecastErrors = ModelUtilities.GenerateHourlyForecastErrors(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(
          numberOfDays * numberOfForecastErrorsInDay,
          forecastErrors.Count);
    }

    [Fact]
    public void correct_error_type_of_forecast_errors_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfForecastErrorsInDay = 23;

      // Act
      var forecastErrors = ModelUtilities.GenerateHourlyForecastErrors(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastErrorsInDay)
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
      var forecastErrors = ModelUtilities.GenerateHourlyForecastErrors(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), forecastErrors.Last().TillDateTime);
    }

    [Theory]
    [InlineData(15, 22, 22)]
    [InlineData(16, 22, 24)]
    [InlineData(15, 24, 24)]
    public void correct_last_request_date_of_forecast_errors_from_many_days(
        short numberOfDays,
        short numberOfForecastErrorsInDay,
        short lastDayRequestInterval)
    {
      // Arrange
      var endRequestDate = _startDate
          .AddDays(numberOfDays)
          .AddHours(lastDayRequestInterval - 24)
          .AddMinutes(ModelUtilities.RequestMinutesOffset);

      // Act
      var forecastErrors = ModelUtilities.GenerateHourlyForecastErrors(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfForecastErrorsInDay)
        .ToList();

      // Assert
      Assert.Equal(
          endRequestDate.ToLocalTime(),
          forecastErrors.Last().RequestDateTime);
    }
  }
}
