using System;

namespace AirlyAnalyzer.Models
{
  public class AirQualityForecastAccuracy
  {
    private DateTime fromDateTime;
    private DateTime tillDateTime;
    private DateTime forecastRequestDateTime;

    public short InstallationId { get; set; }
    public short AirlyCaqiError { get; set; }
    public short Pm25Error { get; set; }
    public short Pm10Error { get; set; }

    public DateTime FromDateTime
    {
      get => fromDateTime.ToLocalTime();
      set => fromDateTime = value.ToUniversalTime();
    }

    public DateTime TillDateTime
    {
      get => tillDateTime.ToLocalTime();
      set => tillDateTime = value.ToUniversalTime();
    }

    public DateTime ForecastRequestDateTime
    {
      get => forecastRequestDateTime.ToLocalTime();
      set => forecastRequestDateTime = value.ToUniversalTime();
    }
  }
}
