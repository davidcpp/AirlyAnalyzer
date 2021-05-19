namespace AirlyAnalyzer.UnitTests.ModelsTests.OpenWeatherApi
{
  using AirlyAnalyzer.Models;
  using Xunit;

  public class OpenWeatherForecastTest
  {
    [Fact]
    public void returns_not_null_hourly_forecast()
    {
      // Act
      var openWeatherForecast = new OpenWeatherForecast();

      // Assert
      Assert.NotNull(openWeatherForecast.HourlyForecast);
    }
  }
}
