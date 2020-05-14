using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AirlyAnalyzer.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AirlyAnalyzer.Models
{
  public class AirQualityDataDownloader
  {
    private const string AirlyApiKeyHeaderName = "apikey";

    private readonly AirlyContext _context;
    private readonly IConfiguration _config;

    private readonly List<short> _installationIdsList;
    private readonly short _minNumberOfMeasurements;

    public AirQualityDataDownloader(
      AirlyContext context,
      IConfiguration config,
      List<short> installationIdsList,
      short minNumberOfMeasurements)
    {
      _context = context;
      _config = config;
      _installationIdsList = installationIdsList;
      _minNumberOfMeasurements = minNumberOfMeasurements;
    }

    public void DownloadAllAirQualityData()
    {
      for (int i = 0; i < _installationIdsList.Count; i++)
      {
        var requestDateTime = DateTime.UtcNow;

        var archiveMeasurements =
          _context.ArchiveMeasurements.Where(x => x.InstallationId == _installationIdsList[i]).ToList();

        var lastMeasurement = archiveMeasurements.Count > 0 ?
          archiveMeasurements.Last() : new AirQualityMeasurement();

        var requestDateTimeDiff = requestDateTime - lastMeasurement.TillDateTime.ToUniversalTime();

        if (requestDateTimeDiff.TotalHours >= _minNumberOfMeasurements)
        {
          var responseMeasurements = DownloadInstallationData(_installationIdsList[i]);

          var newMeasurements = responseMeasurements.History.ConvertToAirQualityMeasurements(
            _installationIdsList[i], requestDateTime);

          var newForecasts = responseMeasurements.Forecast.ConvertToAirQualityForecasts(
            _installationIdsList[i], requestDateTime);

          SaveNewMeasurements(newMeasurements, _installationIdsList[i]);
          SaveNewForecasts(newForecasts, _installationIdsList[i]);
        }
      }
    }

    private Measurements DownloadInstallationData(short installationId)
    {
      string response;

      using (var webClient = new WebClient())
      {
        webClient.BaseAddress = _config.GetValue<string>("AppSettings:AirlyApi:Uri");
        webClient.Headers.Remove(HttpRequestHeader.Accept);
        webClient.Headers.Add(HttpRequestHeader.Accept, _config.GetValue<string>("AirlyApi:ContentType"));
        webClient.Headers.Add(AirlyApiKeyHeaderName, _config.GetValue<string>("AppSettings:AirlyApi:Key"));
        response = webClient.DownloadString(_config.GetValue<string>("AppSettings:AirlyApi:MeasurementsUri")
          + installationId.ToString());
      }
      return JsonConvert.DeserializeObject<Measurements>(response);
    }

    private void SaveNewMeasurements(List<AirQualityMeasurement> newMeasurements, short installationId)
    {
      var archiveMeasurements =
        _context.ArchiveMeasurements.Where(x => x.InstallationId == installationId).ToList();

      // Check if some of measurements there already are in Database
      if (archiveMeasurements.Count > 0)
      {
        var dbLastElement = archiveMeasurements.Last();
        while (newMeasurements.Count > 0 && newMeasurements[0].FromDateTime <= dbLastElement.FromDateTime)
        {
          newMeasurements.RemoveAt(0);
        }
      }

      if (newMeasurements.Count >= _minNumberOfMeasurements)
      {
        _context.ArchiveMeasurements.AddRange(newMeasurements);
        _context.SaveChanges();
      }
    }

    private void SaveNewForecasts(List<AirQualityForecast> newForecasts, short installationId)
    {
      var archiveForecasts =
        _context.ArchiveForecasts.Where(x => x.InstallationId == installationId).ToList();

      // Check if some of forecasts there already are in Database
      if (archiveForecasts.Count > 0)
      {
        var dbLastElement = archiveForecasts.Last();
        while (newForecasts.Count > 0 && newForecasts[0].FromDateTime <= dbLastElement.FromDateTime)
        {
          newForecasts.RemoveAt(0);
        }
      }

      if (newForecasts.Count >= _minNumberOfMeasurements)
      {
        _context.ArchiveForecasts.AddRange(newForecasts);
        _context.SaveChanges();
      }
    }
  }
}
