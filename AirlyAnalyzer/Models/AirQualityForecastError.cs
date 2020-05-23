using System;

namespace AirlyAnalyzer.Models
{
  public class AirQualityForecastError
  {
    private DateTime fromDateTime;
    private DateTime tillDateTime;
    private DateTime requestDateTime;

    public short InstallationId { get; set; }
    public short AirlyCaqiPctError { get; set; }
    public short Pm25PctError { get; set; }
    public short Pm10PctError { get; set; }
    public short AirlyCaqiError { get; set; }
    public short Pm25Error { get; set; }
    public short Pm10Error { get; set; }

    public ForecastErrorType ErrorType { get; set; }

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

    public DateTime RequestDateTime
    {
      get => requestDateTime.ToLocalTime();
      set => requestDateTime = value.ToUniversalTime();
    }
  }

  public enum ForecastErrorType
  {
    Hourly,
    Daily,
    Total,
  }
}
