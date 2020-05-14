using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirlyAnalyzer.Data;
using AirlyAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AirlyAnalyzer.Controllers
{
  public class ForecastController : Controller
  {
    private readonly AirlyContext context;
    private readonly IConfiguration config;
    private readonly List<short> installationIDsList;
    private readonly short minNumberOfMeasurements;

    public ForecastController(AirlyContext context, IConfiguration config)
    {
      this.context = context;
      this.config = config;
      installationIDsList = config.GetSection("AppSettings:AirlyApi:InstallationIds").Get<List<short>>();
      minNumberOfMeasurements =
        config.GetSection("AppSettings:AirlyApi:MinNumberOfMeasurements").Get<short>();
    }

    // GET: Forecast
    public async Task<IActionResult> Index()
    {
      var airQualityDataDownloader = new AirQualityDataDownloader(
        context, config, installationIDsList, minNumberOfMeasurements);

      airQualityDataDownloader.DownloadAllAirQualityData();

      var forecastErrorsCalculation = new ForecastErrorsCalculation(
        context, installationIDsList, minNumberOfMeasurements);

      forecastErrorsCalculation.CalculateAllNewForecastErrors();
      await forecastErrorsCalculation.SaveResultsInDatabase();

      return View(context.ForecastErrors.ToList());
    }
  }
}
