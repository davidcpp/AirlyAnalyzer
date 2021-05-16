namespace AirlyAnalyzer.Client
{
  using System;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Newtonsoft.Json;

  public class OpenWeatherApiDownloader : IOpenWeatherApiDownloader
  {
    private readonly IConfiguration _config;

    protected readonly string _apiKeyParameter;
    protected readonly string _unitsParameter;

    protected readonly string _apiKey;
    protected readonly string _units;

    protected readonly ILogger<OpenWeatherApiDownloader> _logger;
    protected IWebClientAdapter _webClientAdapter;

    public OpenWeatherApiDownloader(
        IConfiguration config,
        IWebClientAdapter webClientAdapter,
        ILogger<OpenWeatherApiDownloader> logger = null)
    {
      _config = config;
      _logger = logger;

      string uri = config.GetValue<string>(
          "AppSettings:OpenWeatherApi:Uri");

      _apiKeyParameter = config.GetValue<string>(
          "AppSettings:OpenWeatherApi:AppIdParameter");

      _unitsParameter = config.GetValue<string>(
          "AppSettings:OpenWeatherApi:UnitsParameter");

      _apiKey = config.GetValue<string>(
          "AppSettings:OpenWeatherApi:Key");

      _units = config.GetValue<string>(
          "AppSettings:OpenWeatherApi:Units");

      _webClientAdapter = webClientAdapter;
      _webClientAdapter.BaseAddress = uri;
    }

    public async Task<OpenWeatherForecast> DownloadHourlyWeatherForecast(
        float latitude, float longtitude)
    {
      try
      {
        const string _exclude = "minutely,daily,current,alerts";

        string forecastUri = _config.GetValue<string>(
            "AppSettings:OpenWeatherApi:OneCallUri");

        string latitudeParameter = _config.GetValue<string>(
            "AppSettings:OpenWeatherApi:LatitudeParameter");

        string longtitudeParameter = _config.GetValue<string>(
            "AppSettings:OpenWeatherApi:LongtitudeParameter");

        string excludeParameter = _config.GetValue<string>(
            "AppSettings:OpenWeatherApi:ExcludeParameter");

        string response = await _webClientAdapter.DownloadStringTaskAsync(
            forecastUri
            + _apiKeyParameter + _apiKey + "&"
            + latitudeParameter + latitude + "&"
            + longtitudeParameter + longtitude + "&"
            + _unitsParameter + _units + "&"
            + excludeParameter + _exclude);

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
        _logger?.LogError(e.StackTrace);
        _logger?.LogError(e.Message);
        _logger?.LogError(e.Source);
        _logger?.LogError("\n");

        return new OpenWeatherForecast();
      }
    }

    public void Dispose()
    {
      _webClientAdapter.Dispose();
    }
  }
}
