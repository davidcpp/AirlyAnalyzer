﻿using System;
using System.Collections.Generic;
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
    private readonly List<short> installationIDsList;

    public ForecastController(AirlyContext context, IConfiguration config)
    {
      this.context = context;
      this.config = config;
      installationIDsList = config.GetSection("AppSettings:AirlyApi:InstallationIds").Get<List<short>>();
    }

    // GET: Forecast
    public async Task<IActionResult> Index()
    {
      var requestDateTime = DateTime.UtcNow;
      var responseMeasurements = DownloadInstallationMeasurements(config, installationIDsList).ToList();

      var historyList = new List<List<AirQualityMeasurement>>();
      var forecastList = new List<List<AirQualityForecast>>();

      for (int i = 0; i < responseMeasurements.Count; i++)
      {
        var history = responseMeasurements[i].History.ConvertToAirQualityMeasurements(
          installationIDsList[i], requestDateTime);
        historyList.Add(history);

        var forecast = responseMeasurements[i].Forecast.ConvertToAirQualityForecasts(
          installationIDsList[i], requestDateTime);
        forecastList.Add(forecast);

        context.SaveNewMeasurements(history, requestDateTime, installationIDsList[i]);
        context.SaveNewForecasts(forecast, requestDateTime, installationIDsList[i]);

        var archiveMeasurements =
          context.ArchiveMeasurements.Where(x => x.InstallationId == installationIDsList[i]).ToList();

        var archiveForecastErrors =
          context.ForecastErrors.Where(x => x.InstallationId == installationIDsList[i]).ToList();

        var lastForecastError = archiveForecastErrors.Count > 0 ?
          archiveForecastErrors.Last() : new AirQualityForecastError();

        var archiveForecasts =
          context.ArchiveForecasts.Where(x => x.InstallationId == installationIDsList[i]).ToList();

        CalculateNewMeasurementsRange(archiveMeasurements,
          archiveForecasts,
          lastForecastError,
          out int measurementsStartIndex,
          out int forecastsStartIndex,
          out int numberOfElements);

        if (numberOfElements > 0)
        {
          var newForecasts = archiveForecasts.GetRange(forecastsStartIndex, numberOfElements);

          var newForecastErrors = newForecasts.CalculateForecastErrors(
            archiveMeasurements.GetRange(measurementsStartIndex, numberOfElements),
            installationIDsList[i]);

          context.ForecastErrors.AddRange(newForecastErrors);
          await context.SaveChangesAsync();
        }
      }
      return View(context.ForecastErrors.ToList());
    }
  }
}
