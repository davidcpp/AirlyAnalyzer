namespace AirlyAnalyzer.Client
{
  using System;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Newtonsoft.Json;

  using AccuWeatherForecast
    = System.Collections.Generic.List<Models.AccuWeatherForecastObject>;

  public class AccuWeatherApiDownloader : IAccuWeatherApiDownloader
  {
    private readonly IConfiguration _config;

    protected readonly string _apiKeyParameter;
    protected readonly string _searchTextParameter;
    protected readonly string _detailsParameter;
    protected readonly string _metricParameter;

    protected readonly string _apiKey;
    protected readonly bool _details;
    protected readonly bool _isMetricUnits;

    protected readonly ILogger<AccuWeatherApiDownloader> _logger;
    protected IWebClientAdapter _webClientAdapter;

    public AccuWeatherApiDownloader(
        IConfiguration config,
        IWebClientAdapter webClientAdapter,
        ILogger<AccuWeatherApiDownloader> logger = null)
    {
      _config = config;
      _logger = logger;

      string uri = config.GetValue<string>(
          "AppSettings:AccuWeatherApi:Uri");

      _apiKeyParameter = config.GetValue<string>(
          "AppSettings:AccuWeatherApi:ApiKeyParameter");

      _searchTextParameter = config.GetValue<string>(
          "AppSettings:AccuWeatherApi:SearchTextParameter");

      _detailsParameter = config.GetValue<string>(
          "AppSettings:AccuWeatherApi:DetailsParameter");

      _metricParameter = config.GetValue<string>(
          "AppSettings:AccuWeatherApi:MetricParameter");

      _apiKey = config.GetValue<string>(
          "AppSettings:AccuWeatherApi:Key");

      _details = config.GetValue<bool>(
          "AppSettings:AccuWeatherApi:Details");

      _isMetricUnits = config.GetValue<bool>(
          "AppSettings:AccuWeatherApi:Metric");

      _webClientAdapter = webClientAdapter;
      _webClientAdapter.BaseAddress = uri;
    }

    public async Task<AccuWeatherForecast> DownloadHourlyWeatherForecast(
        float latitude, float longtitude)
    {
      string forecastUri = _config.GetValue<string>(
          "AppSettings:AccuWeatherApi:Forecast12HoursUri");

      var location = await DownloadLocationInfo(latitude, longtitude);

      try
      {
        string response = await _webClientAdapter.DownloadStringTaskAsync(
            forecastUri + location.Key + "?"
            + _apiKeyParameter + _apiKey + "&"
            + _detailsParameter + _details + "&"
            + _metricParameter + _isMetricUnits);

        return JsonConvert.DeserializeObject<AccuWeatherForecast>(
            response,
            new JsonSerializerSettings()
            {
              NullValueHandling = NullValueHandling.Ignore
            })
          ?? new AccuWeatherForecast();
      }
      catch (Exception e)
      {
        _logger?.LogError(e.StackTrace);
        _logger?.LogError(e.Message);
        _logger?.LogError(e.Source);
        _logger?.LogError("\n");

        return new AccuWeatherForecast();
      }
    }

    private async Task<AccuWeatherLocation> DownloadLocationInfo(
        float latitude, float longtitude)
    {
      string locationUri = _config.GetValue<string>(
          "AppSettings:AccuWeatherApi:LocationUri");

      try
      {
        string response = await _webClientAdapter.DownloadStringTaskAsync(
            locationUri + "?"
            + _apiKeyParameter + _apiKey + "&"
            + _searchTextParameter
            + latitude.ToString() + ", " + longtitude.ToString());

        return JsonConvert.DeserializeObject<AccuWeatherLocation>(
            response,
            new JsonSerializerSettings()
            {
              NullValueHandling = NullValueHandling.Ignore
            })
          ?? new AccuWeatherLocation();
      }
      catch (Exception e)
      {
        _logger?.LogError(e.StackTrace);
        _logger?.LogError(e.Message);
        _logger?.LogError(e.Source);
        _logger?.LogError("\n");

        return new AccuWeatherLocation();
      }
    }

    public void Dispose()
    {
      _webClientAdapter.Dispose();
    }

  }
}
