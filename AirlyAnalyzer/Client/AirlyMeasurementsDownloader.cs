namespace AirlyAnalyzer.Client
{
  using System;
  using System.Diagnostics;
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;

  public class AirlyMeasurementsDownloader : IAirQualityDataDownloader<Measurements>
  {
    private readonly string _airlyApiKeyHeaderName;
    private readonly string _airlyApiKey;
    private readonly string _contentType;
    private readonly string _measurementsUri;
    private readonly string _uri;

    private readonly IWebClientAdapter _webClientAdapter;

    public AirlyMeasurementsDownloader(
        IConfiguration config, IWebClientAdapter webClientAdapter)
    {
      _airlyApiKeyHeaderName = config.GetValue<string>(
          "AppSettings:AirlyApi:KeyHeaderName");

      _airlyApiKey = config.GetValue<string>(
          "AppSettings:AirlyApi:Key");

      _contentType = config.GetValue<string>(
          "AppSettings:AirlyApi:ContentType");

      _measurementsUri = config.GetValue<string>(
          "AppSettings:AirlyApi:MeasurementsUri");

      _uri = config.GetValue<string>(
          "AppSettings:AirlyApi:Uri");

      _webClientAdapter = webClientAdapter;

      _webClientAdapter.BaseAddress = _uri;
      _webClientAdapter.Headers.Remove(HttpRequestHeader.Accept);
      _webClientAdapter.Headers.Add(HttpRequestHeader.Accept, _contentType);
      _webClientAdapter.Headers.Add(_airlyApiKeyHeaderName, _airlyApiKey);
    }

    public async Task<Measurements> DownloadAirQualityData(short installationId)
    {
      try
      {
        string response = await _webClientAdapter.DownloadStringTaskAsync(
            _measurementsUri + installationId.ToString());

        return JsonConvert.DeserializeObject<Measurements>(
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

        return new Measurements();
      }
    }

    public void Dispose()
    {
      _webClientAdapter.Dispose();
    }
  }
}
