namespace AirlyAnalyzer.Client
{
  using System;
  using System.Diagnostics;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;

  public class OpenWeatherDownloader : IDisposable
  {
    private readonly IConfiguration _config;

    protected readonly string _apiKeyParameter;
    protected readonly string _unitsParameter;

    protected readonly string _apiKey;
    protected readonly string _units;

    protected IWebClientAdapter _webClientAdapter;

    public OpenWeatherDownloader(
        IConfiguration config, IWebClientAdapter webClientAdapter)
    {
      _config = config;

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
        string stackTrace = e.StackTrace;
        Debug.WriteLine(stackTrace);
        Debug.WriteLine(e.Message);
        Debug.WriteLine(e.Source);
        Debug.WriteLine("\n");

        return new OpenWeatherForecast();
      }
    }

    public void Dispose()
    {
      _webClientAdapter.Dispose();
    }
  }
}
