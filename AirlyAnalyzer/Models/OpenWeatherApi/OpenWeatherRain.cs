namespace AirlyAnalyzer.Models
{
  using Microsoft.EntityFrameworkCore;
  using Newtonsoft.Json;

  [Owned]
  public class OpenWeatherRain
  {
    [JsonProperty("1h")]
    public float OneHour { get; set; }

    [JsonProperty("3h")]
    public float ThreeHours { get; set; }
  }
}