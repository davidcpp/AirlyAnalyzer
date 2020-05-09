using System;
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

        var archiveForecastAccuracyRates =
          context.ForecastAccuracyRates.Where(x => x.InstallationId == installationIDsList[i]).ToList();

        var lastForecastAccuracy = archiveForecastAccuracyRates.Count > 0 ?
          archiveForecastAccuracyRates.Last() : new AirQualityForecastAccuracy();

        var archiveForecasts =
          context.ArchiveForecasts.Where(x => x.InstallationId == installationIDsList[i]).ToList();

        CalculateNewMeasurementsRange(archiveMeasurements,
          archiveForecasts,
          lastForecastAccuracy,
          out int measurementsStartIndex,
          out int forecastsStartIndex,
          out int numberOfElements);

        if (numberOfElements > 0)
        {
          var newForecasts = archiveForecasts.GetRange(forecastsStartIndex, numberOfElements);

          var newForecastAccuracyRates = newForecasts.CalculateForecastAccuracy(
            archiveMeasurements.GetRange(measurementsStartIndex, numberOfElements),
            installationIDsList[i]);

          context.ForecastAccuracyRates.AddRange(newForecastAccuracyRates);
          await context.SaveChangesAsync();
        }
      }
      return View(context.ForecastAccuracyRates.ToList());
    }
  }
}
