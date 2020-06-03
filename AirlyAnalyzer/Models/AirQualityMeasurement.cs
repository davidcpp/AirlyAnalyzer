namespace AirlyAnalyzer.Models
{
  using System;

  public class AirQualityMeasurement
  {
    private DateTime fromDateTime;
    private DateTime tillDateTime;
    private DateTime requestDateTime;

    public short InstallationId { get; set; }
    public byte AirlyCaqi { get; set; }
    public short Pm1 { get; set; }
    public short Pm25 { get; set; }
    public short Pm10 { get; set; }
    public byte Humidity { get; set; }
    public short Pressure { get; set; }
    public short Temperature { get; set; }

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

    public static explicit operator AirQualityMeasurement(AveragedValues averagedValue)
    {
      var measurement = new AirQualityMeasurement
      {
        FromDateTime = averagedValue.FromDateTime,
        TillDateTime = averagedValue.TillDateTime
      };

      foreach (var index in averagedValue.Indexes)
      {
        measurement.AirlyCaqi = (index.Name == "AIRLY_CAQI")
          ? Convert.ToByte(Math.Ceiling(index.Value)) : (byte)0;
      }

      foreach (var measure in averagedValue.Values)
      {
        switch (measure.Name)
        {
          case "PM1":
            measurement.Pm1 = Convert.ToInt16(Math.Ceiling(measure.Value));
            break;
          case "PM25":
            measurement.Pm25 = Convert.ToInt16(Math.Ceiling(measure.Value));
            break;
          case "PM10":
            measurement.Pm10 = Convert.ToInt16(Math.Ceiling(measure.Value));
            break;
          case "HUMIDITY":
            measurement.Humidity = Convert.ToByte(Math.Ceiling(measure.Value));
            break;
          case "PRESSURE":
            measurement.Pressure = Convert.ToInt16(Math.Ceiling(measure.Value));
            break;
          case "TEMPERATURE":
            measurement.Temperature = Convert.ToByte(Math.Ceiling(measure.Value));
            break;
        }
      }

      return measurement;
    }
  }
}
