namespace AirlyAnalyzer.Client
{
  using System;
  using System.Diagnostics;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;

  public class OpenWeatherForecastDownloader : OpenWeatherDownloader
  {
    private const string _exclude = "minutely,daily,current,alerts";
    private const string _units = "metric";

    private readonly string _latitudeParameter;
    private readonly string _longtitudeParameter;
    private readonly string _excludeParameter;

    public OpenWeatherForecastDownloader(
        IConfiguration config, IWebClientAdapter webClientAdapter)
            : base(config, webClientAdapter)
    {
      _latitudeParameter = config.GetValue<string>(
          "AppSettings:OpenWeatherApi:LatitudeParameter");

      _longtitudeParameter = config.GetValue<string>(
          "AppSettings:OpenWeatherApi:LongtitudeParameter");

      _excludeParameter = config.GetValue<string>(
          "AppSettings:OpenWeatherApi:ExcludeParameter");
    }

    public async Task<OpenWeatherForecast> DownloadWeatherData(
        float latitude, float longtitude)
    {
      try
      {
        string response = await _webClientAdapter.DownloadStringTaskAsync(
            _forecastUri
            + _apiKeyParameter + _apiKey + "&"
            + _latitudeParameter + latitude + "&"
            + _longtitudeParameter + longtitude + "&"
            + _unitsParameter + _units + "&"
            + _excludeParameter + _exclude);

        return JsonConvert.DeserializeObject<OpenWeatherForecast>(
            response,
            new JsonSerializerSettings()
            {
              NullValueHandling = NullValueHandling.Ignore
            })
          ?? new OpenWeatherForecast();
      }
      catch (Exception e)
      {
        string stackTrace = e.StackTrace;
        Debug.WriteLine(stackTrace);
        Debug.WriteLine(e.Message);
        Debug.WriteLine(e.Source);
        Debug.WriteLine("\n");

        return new OpenWeatherForecast();
      }
    }
  }
}
