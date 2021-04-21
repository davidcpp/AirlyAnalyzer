namespace AirlyAnalyzer.Client
{
  using System;
  using System.Diagnostics;
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;

  public class AirlyApiDownloader : IAirlyApiDownloader
  {
    private readonly string _airlyApiKeyHeaderName;
    private readonly string _airlyApiKey;
    private readonly string _contentType;
    private readonly string _uri;

    private readonly IConfiguration _config;
    private readonly IWebClientAdapter _webClientAdapter;

    public AirlyApiDownloader(
        IConfiguration config, IWebClientAdapter webClientAdapter)
    {
      _config = config;

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

    public async Task<Installation>
        DownloadInstallationInfo(short installationId)
    {
      try
      {
        string installationUri = _config.GetValue<string>(
            "AppSettings:AirlyApi:InstallationUri");

        string response = await _webClientAdapter.DownloadStringTaskAsync(
            installationUri + installationId.ToString());

        return JsonConvert.DeserializeObject<Installation>(
            response,
            new JsonSerializerSettings()
            {
              NullValueHandling = NullValueHandling.Ignore
            })
          ?? new Installation();
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

    public async Task<Measurements>
        DownloadInstallationMeasurements(short installationId)
    {
      try
      {
        string measurementsUri = _config.GetValue<string>(
            "AppSettings:AirlyApi:MeasurementsUri");

        string measurementsUriParameters = _config.GetValue<string>(
            "AppSettings:AirlyApi:MeasurementsUriParameters");

        string response = await _webClientAdapter.DownloadStringTaskAsync(
            measurementsUri
            + installationId.ToString()
            + measurementsUriParameters);

        return JsonConvert.DeserializeObject<Measurements>(
            response,
            new JsonSerializerSettings()
            {
              NullValueHandling = NullValueHandling.Ignore
            })
          ?? new Measurements();
      }
      catch (Exception e)
      {
        string stackTrace = e.StackTrace;
        Debug.WriteLine(stackTrace);
        Debug.WriteLine(e.Message);
        Debug.WriteLine(e.Source);
        Debug.WriteLine("\n");

        return new Measurements();
      }
    }

    public void Dispose()
    {
      _webClientAdapter.Dispose();
    }
  }
}
