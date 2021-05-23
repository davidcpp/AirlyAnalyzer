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
  }
}

