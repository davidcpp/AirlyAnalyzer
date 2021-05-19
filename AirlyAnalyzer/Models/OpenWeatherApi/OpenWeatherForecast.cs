namespace AirlyAnalyzer.Models
{
  using System.Collections.Generic;
  using Newtonsoft.Json;

  public class OpenWeatherForecast
  {
    public OpenWeatherForecast()
    {
      HourlyForecast = new List<OpenWeatherForecastObject>();
    }

    [JsonProperty("timezone_offset")]
    public int TimeZoneOffset { get; set; }

    [JsonProperty("hourly")]
    public List<OpenWeatherForecastObject> HourlyForecast { get; set; }
  }
}
