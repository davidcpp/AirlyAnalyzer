﻿namespace AirlyAnalyzer.Models
{
  using Newtonsoft.Json;

  public class OpenWeatherForecastObject
  {
    public OpenWeatherForecastObject()
    {
      Rain = new OpenWeatherRain();
    }

    [JsonProperty("dt")]
    public long Time { get; set; }

    [JsonProperty("clouds")]
    public byte Cloudiness { get; set; }

    public byte Humidity { get; set; }

    public short Pressure { get; set; }

    [JsonProperty("pop")]
    public float PropababilityOfPrecipitation { get; set; }

    [JsonProperty("rain")]
    public OpenWeatherRain Rain { get; set; }

    [JsonProperty("temp")]
    public float Temperature { get; set; }

    public int Visibility { get; set; }

    [JsonProperty("wind_speed")]
    public float WindSpeed { get; set; }

    [JsonProperty("wind_deg")]
    public short WindDirection { get; set; }

    [JsonProperty("wind_gust")]
    public float WindGust { get; set; }

    public float AirlyCaqi { get; set; }
  }
}
