namespace AirlyAnalyzer.Models
{
  using System.ComponentModel;

  public class AirQualityForecastError : AirQualityObject
  {
    [DisplayName("Caqi Error [%]")]
    public short AirlyCaqiPct { get; set; }

    [DisplayName("PM2.5 Error [%]")]
    public short Pm25Pct { get; set; }

    [DisplayName("PM10 Error [%]")]
    public short Pm10Pct { get; set; }

    [DisplayName("Error Period")]
    public ForecastErrorPeriod Period { get; set; }
  }

  public enum ForecastErrorPeriod
  {
    Hour,
    Day,
    Total,
  }
}
