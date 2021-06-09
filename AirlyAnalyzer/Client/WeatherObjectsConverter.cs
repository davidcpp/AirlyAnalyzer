namespace AirlyAnalyzer.Client
{
  using System;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Models.Weather;

  public static class WeatherObjectsConverter
  {
    public static WeatherMeasurement ConvertToWeatherMeasurement(
        this OpenWeatherForecastObject weatherForecastItem, int installationId)
    {
      var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
          .AddSeconds(weatherForecastItem.Time);

      return new WeatherMeasurement()
      {
        Year = (short)dateTime.Year,
        Month = (byte)dateTime.Month,
        Day = (byte)dateTime.Day,
        Hour = (byte)dateTime.Hour,
        InstallationId = installationId,
        CloudCover = weatherForecastItem.Cloudiness,
        Humidity = weatherForecastItem.Humidity,
        Pressure = weatherForecastItem.Pressure,
        Temperature = weatherForecastItem.Temperature,
        Visibility = weatherForecastItem.Visibility,
        WindSpeed = weatherForecastItem.WindSpeed,
        WindDirection = weatherForecastItem.WindDirection,
        WindGust = weatherForecastItem.WindGust,
      };
    }

    public static WeatherMeasurement ConvertToWeatherMeasurement(
        this AccuWeatherForecastObject weatherForecastItem, int installationId)
    {
      int visibility = 0;
      int windSpeed = 0;

      if (weatherForecastItem.Visibility.Unit == "km")
      {
        visibility = (int)weatherForecastItem.Visibility.Value * 1000;
      }
      else if (weatherForecastItem.Visibility.Unit == "m")
      {
        visibility = (int)weatherForecastItem.Visibility.Value;
      }

      if (weatherForecastItem.Wind.Speed.Unit == "km/h")
      {
        windSpeed = (int)weatherForecastItem.Wind.Speed.Value * 1000;
      }
      else if (weatherForecastItem.Wind.Speed.Unit == "m")
      {
        windSpeed = (int)weatherForecastItem.Wind.Speed.Value;
      }

      return new WeatherMeasurement()
      {
        Year = (short)weatherForecastItem.DateTime.Year,
        Month = (byte)weatherForecastItem.DateTime.Month,
        Day = (byte)weatherForecastItem.DateTime.Day,
        Hour = (byte)weatherForecastItem.DateTime.Hour,
        InstallationId = installationId,
        Humidity = weatherForecastItem.RelativeHumidity,
        Temperature = weatherForecastItem.Temperature.Value,
        Visibility = visibility,
        WindSpeed = windSpeed,
      };
    }
  }
}

