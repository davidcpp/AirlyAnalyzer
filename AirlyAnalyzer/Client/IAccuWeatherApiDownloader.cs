namespace AirlyAnalyzer.Client
{
  using System;
  using System.Threading.Tasks;

  using AccuWeatherForecast
    = System.Collections.Generic.List<Models.AccuWeatherForecastObject>;

  public interface IAccuWeatherApiDownloader : IDisposable
  {
    public Task<AccuWeatherForecast> DownloadHourlyWeatherForecast(float latitude, float longtitude);
  }
}
