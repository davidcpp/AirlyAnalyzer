﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AirlyAnalyzer.Data;
using AirlyAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static AirlyAnalyzer.Models.ModelExtensions;

namespace AirlyAnalyzer.Controllers
{
  public class ForecastController : Controller
  {
    private readonly AirlyContext context;
    private readonly IConfiguration config;

    public ForecastController(AirlyContext context, IConfiguration config)
    {
      this.context = context;
      this.config = config;
    }

    // GET: Forecast
    public async Task<IActionResult> Index()
    {
      var requestDateTime = DateTime.UtcNow;
      var responseMeasurements = DownloadInstallationMeasurements(config);

      var history = responseMeasurements.History.ConvertToAirQualityMeasurements(
        config.GetValue<short>("AppSettings:AirlyApi:InstallationId"),
        requestDateTime);

      var forecast = responseMeasurements.Forecast.ConvertToAirQualityForecasts(
        config.GetValue<short>("AppSettings:AirlyApi:InstallationId"),
        requestDateTime);

      context.SaveNewMeasurements(history, requestDateTime);
      context.SaveNewForecasts(forecast, requestDateTime);

      var archiveMeasurements = context.ArchiveMeasurements.ToList();
      var archiveForecasts = context.ArchiveForecasts.ToList();

      var lastForecastAccuracy = context.ForecastAccuracyRates.ToList().Count > 0 ?
        context.ForecastAccuracyRates.ToList().Last() : new AirQualityForecastAccuracy();

      int i = archiveMeasurements.Count - 1;
      while (i >= 0 && lastForecastAccuracy.TillDateTime < archiveMeasurements[i].TillDateTime)
      {
        i--;
      }

      int j = archiveForecasts.Count - 1;
      while (j >= 0 && lastForecastAccuracy.TillDateTime < archiveForecasts[j].TillDateTime)
      {
        j--;
      }

      int numberOfElements = archiveMeasurements.Count - (i + 1);

      if (numberOfElements > 0)
      {
        var newForecasts = archiveForecasts.GetRange(j + 1, numberOfElements);

        var newForecastAccuracyRates = newForecasts.CalculateForecastAccuracy(
          archiveMeasurements.GetRange(i + 1, numberOfElements),
          config.GetValue<short>("AppSettings:AirlyApi:InstallationId"));

        context.ForecastAccuracyRates.AddRange(newForecastAccuracyRates);
        await context.SaveChangesAsync();
      }

      return View(context.ForecastAccuracyRates.ToList());
    }
  }
}
