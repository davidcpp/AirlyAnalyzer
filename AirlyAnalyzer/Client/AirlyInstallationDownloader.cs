namespace AirlyAnalyzer.Client
{
  using System;
  using System.Diagnostics;
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;

  public class AirlyInstallationDownloader : IAirQualityDataDownloader<Installation>
  {
    private readonly string _airlyApiKeyHeaderName;
    private readonly string _airlyApiKey;
    private readonly string _contentType;
    private readonly string _installationUri;
    private readonly string _uri;

    private readonly IWebClientAdapter _webClient;

    public AirlyInstallationDownloader(
        IConfiguration config, IWebClientAdapter webClient)
    {
      _airlyApiKeyHeaderName = config.GetValue<string>(
          "AppSettings:AirlyApi:KeyHeaderName");

      _airlyApiKey = config.GetValue<string>(
          "AppSettings:AirlyApi:Key");

      _contentType = config.GetValue<string>(
          "AppSettings:AirlyApi:ContentType");

      _installationUri = config.GetValue<string>(
          "AppSettings:AirlyApi:InstallationUri");

      _uri = config.GetValue<string>(
          "AppSettings:AirlyApi:Uri");

      _webClient = webClient;

      _webClient.BaseAddress = _uri;
      _webClient.Headers.Remove(HttpRequestHeader.Accept);
      _webClient.Headers.Add(HttpRequestHeader.Accept, _contentType);
      _webClient.Headers.Add(_airlyApiKeyHeaderName, _airlyApiKey);
    }

    public async Task<Installation> DownloadAirQualityData(short installationId)
    {

      try
      {
        string response = await _webClient.DownloadStringTaskAsync(
            _installationUri + installationId.ToString());

        return JsonConvert.DeserializeObject<Installation>(
            response,
            new JsonSerializerSettings()
            {
              NullValueHandling = NullValueHandling.Ignore
            });
      }
      catch (Exception e)
      {
        string stackTrace = e.StackTrace;
        Debug.WriteLine(stackTrace);
        Debug.WriteLine(e.Message);
        Debug.WriteLine(e.Source);
        Debug.WriteLine("\n");

        return new Installation();
      }
    }

    public void Dispose()
    {
      _webClient.Dispose();
    }
  }
}
