namespace AirlyAnalyzer.Models
{
  using System;
  using System.Diagnostics;
  using System.Net;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;

  public class AirQualityDataDownloader
  {
    private readonly string _airlyApiKeyHeaderName;
    private readonly string _airlyApiKey;
    private readonly string _contentType;
    private readonly string _measurementsUri;
    private readonly string _uri;

    public AirQualityDataDownloader(IConfiguration config)
    {
      _airlyApiKeyHeaderName = config.GetValue<string>("AppSettings:AirlyApi:KeyHeaderName");
      _airlyApiKey = config.GetValue<string>("AppSettings:AirlyApi:Key");
      _contentType = config.GetValue<string>("AppSettings:AirlyApi:ContentType");
      _measurementsUri = config.GetValue<string>("AppSettings:AirlyApi:MeasurementsUri");
      _uri = config.GetValue<string>("AppSettings:AirlyApi:Uri");
    }

    public Measurements DownloadAirQualityData(short installationId)
    {
      using (var webClient = new WebClient())
      {
        webClient.BaseAddress = _uri;
        webClient.Headers.Remove(HttpRequestHeader.Accept);
        webClient.Headers.Add(HttpRequestHeader.Accept, _contentType);
        webClient.Headers.Add(_airlyApiKeyHeaderName, _airlyApiKey);

        try
        {
          string response = webClient.DownloadString(_measurementsUri + installationId.ToString());

          return JsonConvert.DeserializeObject<Measurements>(response, new JsonSerializerSettings()
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
    }
  }
}
