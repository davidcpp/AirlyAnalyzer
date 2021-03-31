namespace AirlyAnalyzer.Models
{
  using System;

  public class AirQualityForecast : AirQualityObject
  {
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
        if (index.Name == "AIRLY_CAQI")
        {
          forecast.AirlyCaqi = Convert.ToInt16(Math.Ceiling(index.Value));
        }
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
