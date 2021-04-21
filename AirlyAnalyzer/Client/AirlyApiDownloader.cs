namespace AirlyAnalyzer.Client
{
  using System.Net;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Configuration;

  public abstract class AirlyApiDownloader<T> : IAirQualityDataDownloader<T>
  {
    private readonly string _airlyApiKeyHeaderName;
    private readonly string _airlyApiKey;
    private readonly string _contentType;
    private readonly string _uri;

    protected readonly IWebClientAdapter _webClientAdapter;

    protected AirlyApiDownloader(
        IConfiguration config, IWebClientAdapter webClientAdapter)
    {
      _airlyApiKeyHeaderName = config.GetValue<string>(
          "AppSettings:AirlyApi:KeyHeaderName");

      _airlyApiKey = config.GetValue<string>(
          "AppSettings:AirlyApi:Key");

      _contentType = config.GetValue<string>(
          "AppSettings:AirlyApi:ContentType");

      _uri = config.GetValue<string>(
          "AppSettings:AirlyApi:Uri");

      _webClientAdapter = webClientAdapter;

      _webClientAdapter.BaseAddress = _uri;
      _webClientAdapter.Headers.Remove(HttpRequestHeader.Accept);
      _webClientAdapter.Headers.Add(HttpRequestHeader.Accept, _contentType);
      _webClientAdapter.Headers.Add(_airlyApiKeyHeaderName, _airlyApiKey);
    }

    public abstract Task<T> DownloadAirQualityData(short installationId);

    public void Dispose()
    {
      _webClientAdapter.Dispose();
    }
  }
}
