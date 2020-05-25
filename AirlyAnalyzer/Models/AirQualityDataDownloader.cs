﻿using System;
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
    private readonly string airlyApiKeyHeaderName;
    private readonly string airlyApiKey;
    private readonly string contentType;
    private readonly string measurementsUri;
    private readonly string uri;

    private readonly AirlyContext _context;

    private readonly List<short> _installationIdsList;
    private readonly short _minNumberOfMeasurements;

    public AirQualityDataDownloader(
      AirlyContext context,
      IConfiguration config,
      List<short> installationIdsList,
      short minNumberOfMeasurements)
    {
      _context = context;
      _installationIdsList = installationIdsList;
      _minNumberOfMeasurements = minNumberOfMeasurements;

      airlyApiKeyHeaderName = config.GetValue<string>("AppSettings:AirlyApi:KeyHeaderName");
      airlyApiKey = config.GetValue<string>("AppSettings:AirlyApi:Key");
      contentType = config.GetValue<string>("AppSettings:AirlyApi:ContentType");
      measurementsUri = config.GetValue<string>("AppSettings:AirlyApi:MeasurementsUri");
      uri = config.GetValue<string>("AppSettings:AirlyApi:Uri");
    }

    public void DownloadAllAirQualityData()
    {
      for (int i = 0; i < _installationIdsList.Count; i++)
      {
        var requestDateTime = DateTime.UtcNow;
        var lastMeasurementDate = DateTime.MinValue;

        if (_context.ArchiveMeasurements.Any())
        {
          lastMeasurementDate = _context.ArchiveMeasurements
            .Where(e => e.InstallationId == _installationIdsList[i])
            .OrderByDescending(e => e.FromDateTime)
            .Select(e => e.TillDateTime)
            .First();
        }

        var requestDateTimeDiff = requestDateTime - lastMeasurementDate;

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
      using (var webClient = new WebClient())
      {
        webClient.BaseAddress = uri;
        webClient.Headers.Remove(HttpRequestHeader.Accept);
        webClient.Headers.Add(HttpRequestHeader.Accept, contentType);
        webClient.Headers.Add(airlyApiKeyHeaderName, airlyApiKey);
        string response = webClient.DownloadString(measurementsUri + installationId.ToString());

        return JsonConvert.DeserializeObject<Measurements>(response);
      }
    }

    private void SaveNewMeasurements(List<AirQualityMeasurement> newMeasurements, short installationId)
    {
      var lastMeasurementDate = DateTime.MinValue;

      // Check if some of measurements there already are in Database
      if (_context.ArchiveMeasurements.Any())
      {
        lastMeasurementDate = _context.ArchiveMeasurements
          .Where(e => e.InstallationId == installationId)
          .OrderByDescending(e => e.FromDateTime)
          .Select(e => e.FromDateTime)
          .First();
      }

      while (newMeasurements.Count > 0 && newMeasurements[0].FromDateTime <= lastMeasurementDate)
      {
        newMeasurements.RemoveAt(0);
      }

      if (newMeasurements.Count >= _minNumberOfMeasurements)
      {
        _context.ArchiveMeasurements.AddRange(newMeasurements);
        _context.SaveChanges();
      }
    }

    private void SaveNewForecasts(List<AirQualityForecast> newForecasts, short installationId)
    {
      var lastForecastDate = DateTime.MinValue;

      // Check if some of forecasts there already are in Database
      if (_context.ArchiveForecasts.Any())
      {
        lastForecastDate = _context.ArchiveForecasts
          .Where(e => e.InstallationId == installationId)
          .OrderByDescending(e => e.FromDateTime)
          .Select(e => e.FromDateTime)
          .First();
      }

      while (newForecasts.Count > 0 && newForecasts[0].FromDateTime <= lastForecastDate)
      {
        newForecasts.RemoveAt(0);
      }

      if (newForecasts.Count >= _minNumberOfMeasurements)
      {
        _context.ArchiveForecasts.AddRange(newForecasts);
        _context.SaveChanges();
      }
    }
  }
}
