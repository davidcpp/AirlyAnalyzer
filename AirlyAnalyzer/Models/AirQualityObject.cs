namespace AirlyAnalyzer.Models
{
  using System;

  public abstract class AirQualityObject
  {
    private DateTime fromDateTime;
    private DateTime tillDateTime;
    private DateTime requestDateTime;

    public short InstallationId { get; set; }
    public short AirlyCaqi { get; set; }
    public short Pm25 { get; set; }
    public short Pm10 { get; set; }

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
}
