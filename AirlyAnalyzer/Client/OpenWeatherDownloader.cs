namespace AirlyAnalyzer.Client
{
  using System;
  using Microsoft.Extensions.Configuration;

  public abstract class OpenWeatherDownloader : IDisposable
  {
    protected readonly string _apiKeyParameter;
    protected readonly string _unitsParameter;

    protected readonly string _apiKey;
    protected readonly string _units;

    protected IWebClientAdapter _webClientAdapter;

    protected OpenWeatherDownloader(
        IConfiguration config, IWebClientAdapter webClientAdapter)
    {
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

    public void Dispose()
    {
      _webClientAdapter.Dispose();
    }
  }
}
