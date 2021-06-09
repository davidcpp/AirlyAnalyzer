namespace AirlyAnalyzer.Models
{
  using System;

  public class AccuWeatherForecastObject
  {
    public DateTime DateTime { get; set; }

    public AccuWeatherValue Temperature { get; set; }

    public AccuWeatherWind Wind { get; set; }

    public byte RelativeHumidity { get; set; }

    public AccuWeatherValue Visibility { get; set; }
  }
}