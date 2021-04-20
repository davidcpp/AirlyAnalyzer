namespace AirlyAnalyzer.Models
{
  using Newtonsoft.Json;

  public class OpenWeatherForecastObject
  {
    [JsonProperty("dt")]
    public long Time { get; set; }

    [JsonProperty("Clouds")]
    public byte Cloudiness { get; set; }

    public byte Humidity { get; set; }

    public short Pressure { get; set; }

    [JsonProperty("pop")]
    public float PropababilityOfPrecipitation { get; set; }

    [JsonProperty("rain.1h")]
    public float Rain { get; set; }

    [JsonProperty("temp")]
    public short Temperature { get; set; }

    public int Visibility { get; set; }

    [JsonProperty("wind_speed")]
    public float WindSpeed { get; set; }

    [JsonProperty("wind_deg")]
    public short WindBearing { get; set; }

    [JsonProperty("wind_gust")]
    public float WindGust { get; set; }
  }
}
