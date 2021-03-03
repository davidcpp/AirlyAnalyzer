namespace AirlyAnalyzer.Models
{
  public class AirQualityForecastError : AirQualityObject
  {
    public short AirlyCaqiPct { get; set; }
    public short Pm25Pct { get; set; }
    public short Pm10Pct { get; set; }

    public ForecastErrorType ErrorType { get; set; }
  }

  public enum ForecastErrorType
  {
    Hourly,
    Daily,
    Total,
  }
}
