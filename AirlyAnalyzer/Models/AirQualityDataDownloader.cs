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

    private readonly DatabaseHelper _databaseHelper;

    private readonly short _minNumberOfMeasurements;

    public AirQualityDataDownloader(
      DatabaseHelper databaseHelper, IConfiguration config, short minNumberOfMeasurements)
    {
      _databaseHelper = databaseHelper;
      _minNumberOfMeasurements = minNumberOfMeasurements;

      _airlyApiKeyHeaderName = config.GetValue<string>("AppSettings:AirlyApi:KeyHeaderName");
      _airlyApiKey = config.GetValue<string>("AppSettings:AirlyApi:Key");
      _contentType = config.GetValue<string>("AppSettings:AirlyApi:ContentType");
      _measurementsUri = config.GetValue<string>("AppSettings:AirlyApi:MeasurementsUri");
      _uri = config.GetValue<string>("AppSettings:AirlyApi:Uri");
    }

    public (List<AirQualityMeasurement>, List<AirQualityForecast>)
      DownloadAllAirQualityData(short installationId)
    {
      var requestDateTime = DateTime.UtcNow;
      var lastMeasurementDate = _databaseHelper.SelectLastMeasurementDate(installationId);

      var requestDateTimeDiff = requestDateTime - lastMeasurementDate;

      var newMeasurements = new List<AirQualityMeasurement>();
      var newForecasts = new List<AirQualityForecast>();

      if (requestDateTimeDiff.TotalHours >= _minNumberOfMeasurements)
      {
        var responseMeasurements = DownloadInstallationData(installationId);

        newMeasurements = responseMeasurements.History.ConvertToAirQualityMeasurements(
          installationId, requestDateTime);

        newForecasts = responseMeasurements.Forecast.ConvertToAirQualityForecasts(
          installationId, requestDateTime);
      }

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
        string response = webClient.DownloadString(_measurementsUri + installationId.ToString());

        try
        {
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
