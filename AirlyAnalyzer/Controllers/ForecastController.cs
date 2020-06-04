namespace AirlyAnalyzer.Controllers
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;

  public class ForecastController : Controller
  {
    private readonly AirlyContext _context;
    private readonly IConfiguration _config;

    private readonly List<short> _installationIDsList;
    private readonly short _minNumberOfMeasurements;

    private readonly DatabaseHelper _databaseHelper;

    public ForecastController(AirlyContext context, IConfiguration config)
    {
      _context = context;
      _config = config;

      _installationIDsList = config.GetSection("AppSettings:AirlyApi:InstallationIds").Get<List<short>>();
      _minNumberOfMeasurements = config.GetValue<short>("AppSettings:AirlyApi:MinNumberOfMeasurements");

      _databaseHelper = new DatabaseHelper(_context, _minNumberOfMeasurements);
    }

    // GET: Forecast
    public async Task<IActionResult> Index()
    {
      var airQualityDataDownloader
        = new AirQualityDataDownloader(_databaseHelper, _config, _minNumberOfMeasurements);

      foreach (short installationId in _installationIDsList)
      {
        var (newMeasurements, newForecasts)
          = airQualityDataDownloader.DownloadAllAirQualityData(installationId);

        await _databaseHelper.SaveNewMeasurements(newMeasurements, installationId);
        await _databaseHelper.SaveNewForecasts(newForecasts, installationId);
      }

      var forecastErrorsCalculation = new ForecastErrorsCalculation(
        _databaseHelper, _config, _installationIDsList, _minNumberOfMeasurements);

      var newForecastErrors = forecastErrorsCalculation.CalculateAllNewForecastErrors();
      if (newForecastErrors.Count > 0)
      {
        await _databaseHelper.SaveForecastErrors(newForecastErrors);
        var newTotalForecastErrors = forecastErrorsCalculation.CalculateAllTotalForecastErrors();
        await _databaseHelper.SaveForecastErrors(newTotalForecastErrors);
      }

      return View(_context.ForecastErrors.ToList());
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _context.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
