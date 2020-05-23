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
    private readonly AirlyContext _context;
    private readonly IConfiguration _config;
    private readonly List<short> _installationIDsList;
    private readonly short _minNumberOfMeasurements;

    public ForecastController(AirlyContext context, IConfiguration config)
    {
      _context = context;
      _config = config;
      _installationIDsList = config.GetSection("AppSettings:AirlyApi:InstallationIds").Get<List<short>>();
      _minNumberOfMeasurements = config.GetValue<short>("AppSettings:AirlyApi:MinNumberOfMeasurements");
    }

    // GET: Forecast
    public async Task<IActionResult> Index()
    {
      var airQualityDataDownloader = new AirQualityDataDownloader(
        _context, _config, _installationIDsList, _minNumberOfMeasurements);

      airQualityDataDownloader.DownloadAllAirQualityData();

      var forecastErrorsCalculation = new ForecastErrorsCalculation(
        _context, _config, _installationIDsList, _minNumberOfMeasurements);

      if (forecastErrorsCalculation.CalculateAllNewForecastErrors() > 0)
      {
        await forecastErrorsCalculation.SaveResultsInDatabase();
        forecastErrorsCalculation.CalculateAllTotalForecastErrors();
        await forecastErrorsCalculation.SaveResultsInDatabase();
      }

      return View(_context.ForecastErrors.ToList());
    }
  }
}
