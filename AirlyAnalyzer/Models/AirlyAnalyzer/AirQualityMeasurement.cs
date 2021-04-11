namespace AirlyAnalyzer.Models
{
  using System;

  public class AirQualityMeasurement : AirQualityObject
  {
    public short Pm1 { get; set; }

    public byte Humidity { get; set; }

    public short Pressure { get; set; }

    public short Temperature { get; set; }

    public float WindSpeed { get; set; }

    public short WindBearing { get; set; }

    public static explicit operator AirQualityMeasurement(
        AveragedValues averagedValue)
    {
      var measurement = new AirQualityMeasurement
      {
        FromDateTime = averagedValue.FromDateTime,
        TillDateTime = averagedValue.TillDateTime
      };

      foreach (var index in averagedValue.Indexes)
      {
        if (index.Name == "AIRLY_CAQI")
        {
          measurement.AirlyCaqi = Convert.ToInt16(Math.Ceiling(index.Value));
        }
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
            measurement.Temperature = Convert.ToInt16(Math.Ceiling(measure.Value));
            break;
          case "WIND_SPEED":
            measurement.WindSpeed = (float)measure.Value;
            break;
          case "WIND_BEARING":
            measurement.WindBearing = Convert.ToInt16(measure.Value);
            break;
        }
      }

      return measurement;
    }
  }
}
