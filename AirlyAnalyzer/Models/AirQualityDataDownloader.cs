namespace AirlyAnalyzer.Models
{
  using System;
  using System.Collections.Generic;
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

    public (List<AirQualityMeasurement>, List<AirQualityForecast>)
      DownloadAirQualityData(short installationId)
    {
      var requestDateTime = DateTime.UtcNow;
      var responseMeasurements = DownloadInstallationData(installationId);

      var newMeasurements = responseMeasurements.History.ConvertToAirQualityMeasurements(
        installationId, requestDateTime);

      var newForecasts = responseMeasurements.Forecast.ConvertToAirQualityForecasts(
        installationId, requestDateTime);

      return (newMeasurements, newForecasts);
    }

    private Measurements DownloadInstallationData(short installationId)
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
          return JsonConvert.DeserializeObject<Measurements>(response);
        }
        catch (Exception)
        {
          return new Measurements();
        }
      }
    }
  }
}
