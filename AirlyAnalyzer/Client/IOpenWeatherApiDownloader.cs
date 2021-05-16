namespace AirlyAnalyzer.Client
{
  using System;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;

  public interface IOpenWeatherApiDownloader : IDisposable
  {
    public Task<OpenWeatherForecast> DownloadHourlyWeatherForecast(float latitude, float longtitude);
  }
}
