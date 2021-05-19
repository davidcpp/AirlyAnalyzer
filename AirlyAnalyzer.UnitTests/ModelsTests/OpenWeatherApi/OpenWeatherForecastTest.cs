namespace AirlyAnalyzer.UnitTests.ModelsTests.OpenWeatherApi
{
  using AirlyAnalyzer.Models;
  using Xunit;

  public class OpenWeatherForecastTest
  {
    [Fact]
    public void not_null_hourly_forecast_property_for_new_object()
    {
      // Act
      var openWeatherForecast = new OpenWeatherForecast();

      // Assert
      Assert.NotNull(openWeatherForecast.HourlyForecast);
    }

    [Fact]
    public void time_zone_offset_property_equals_zero_for_new_object()
    {
      // Act
      var openWeatherForecast = new OpenWeatherForecast();

      // Assert
      Assert.Equal(0, openWeatherForecast.TimeZoneOffset);
    }
  }
}
