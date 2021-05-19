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
  }
}
