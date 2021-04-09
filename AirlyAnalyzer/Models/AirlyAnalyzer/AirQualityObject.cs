namespace AirlyAnalyzer.Models
{
  using System;
  using System.ComponentModel;

  public abstract class AirQualityObject
  {
    private DateTime fromDateTime;
    private DateTime tillDateTime;
    private DateTime requestDateTime;

    [DisplayName("Installation Id")]
    public short InstallationId { get; set; }

    [DisplayName("Installation Address")]
    public string InstallationAddress { get; set; }

    [DisplayName("Caqi")]
    public short AirlyCaqi { get; set; }

    [DisplayName("PM 2.5")]
    public short Pm25 { get; set; }

    [DisplayName("PM 10")]
    public short Pm10 { get; set; }

    [DisplayName("Start Time")]
    public DateTime FromDateTime
    {
      get => fromDateTime.ToLocalTime();
      set => fromDateTime = value.ToUniversalTime();
    }

    [DisplayName("Time")]
    public DateTime TillDateTime
    {
      get => tillDateTime.ToLocalTime();
      set => tillDateTime = value.ToUniversalTime();
    }

    [DisplayName("Request Time")]
    public DateTime RequestDateTime
    {
      get => requestDateTime.ToLocalTime();
      set => requestDateTime = value.ToUniversalTime();
    }
  }
}
