namespace AirlyAnalyzer.Models
{
  using System;

  public class AirQualityForecast
  {
    private DateTime fromDateTime;
    private DateTime tillDateTime;
    private DateTime requestDateTime;

    public short InstallationId { get; set; }
    public byte AirlyCaqi { get; set; }
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

    public static explicit operator AirQualityForecast(
        AveragedValues averagedValue)
    {
      var forecast = new AirQualityForecast
      {
        FromDateTime = averagedValue.FromDateTime,
        TillDateTime = averagedValue.TillDateTime
      };

      foreach (var index in averagedValue.Indexes)
      {
        forecast.AirlyCaqi = (index.Name == "AIRLY_CAQI") ?
          Convert.ToByte(Math.Ceiling(index.Value)) : (byte)0;
      }

      foreach (var measure in averagedValue.Values)
      {
        switch (measure.Name)
        {
          case "PM25":
            forecast.Pm25 = Convert.ToInt16(Math.Ceiling(measure.Value));
            break;
          case "PM10":
            forecast.Pm10 = Convert.ToInt16(Math.Ceiling(measure.Value));
            break;
        }
      }

      return forecast;
    }
  }
}
