namespace AirlyAnalyzer.Controllers
{
  using System;
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
    private readonly short _idForAllInstallations;
    private readonly short _minNumberOfMeasurements;

    private readonly DatabaseHelper _databaseHelper;
    private readonly ForecastErrorsCalculation _forecastErrorsCalculation;
    private readonly AirQualityDataDownloader _airQualityDataDownloader;

    public ForecastController(AirlyContext context, IConfiguration config)
    {
      _context = context;
      _config = config;

      _minNumberOfMeasurements = config.GetValue<short>("AppSettings:AirlyApi:MinNumberOfMeasurements");
      _installationIDsList = config.GetSection("AppSettings:AirlyApi:InstallationIds").Get<List<short>>();
      _idForAllInstallations = _config.GetValue<short>("AppSettings:AirlyApi:IdForAllInstallations");

      _airQualityDataDownloader = new AirQualityDataDownloader(_config);
      _databaseHelper = new DatabaseHelper(_context, _minNumberOfMeasurements);
      _forecastErrorsCalculation = new ForecastErrorsCalculation(_minNumberOfMeasurements);
    }

    // GET: Forecast
    public async Task<IActionResult> Index()
    {
      await DownloadAndSaveAirQualityData();
      await CalculateAndSaveForecastErrors();
      await CalculateAndSaveTotalForecastErrors();

      return View(_context.ForecastErrors.ToList());
    }

    private async Task DownloadAndSaveAirQualityData()
    {
      // Downloading and saving new data in database
      foreach (short installationId in _installationIDsList)
      {
        var lastMeasurementDate = _databaseHelper.SelectLastMeasurementDate(installationId);

        if ((DateTime.UtcNow - lastMeasurementDate).TotalHours >= _minNumberOfMeasurements)
        {
          var (newMeasurements, newForecasts)
            = _airQualityDataDownloader.DownloadAirQualityData(installationId);

          await _databaseHelper.SaveNewMeasurements(newMeasurements, installationId);
          await _databaseHelper.SaveNewForecasts(newForecasts, installationId);
        }
      }
    }

    private async Task CalculateAndSaveForecastErrors()
    {
      // Calculating and saving new daily and hourly forecast errors in database
      foreach (short installationId in _installationIDsList)
      {
        _databaseHelper.SelectDataToProcessing(
          installationId, out var newArchiveMeasurements, out var newArchiveForecasts);

        var newForecastErrors = _forecastErrorsCalculation.CalculateNewForecastErrors(
          installationId, newArchiveMeasurements, newArchiveForecasts);

        await _databaseHelper.SaveForecastErrors(newForecastErrors);
      }
    }

    private async Task CalculateAndSaveTotalForecastErrors()
    {
      var newTotalForecastErrors = new List<AirQualityForecastError>();

      // Calculating total forecast errors for each installation
      foreach (short installationId in _installationIDsList)
      {
        var dailyForecastErrors = _databaseHelper.SelectDailyForecastErrors(installationId);

        if (dailyForecastErrors.Count > 0)
        {
          var installationForecastError
            = _forecastErrorsCalculation.CalculateTotalForecastError(dailyForecastErrors, installationId);

          newTotalForecastErrors.Add(installationForecastError);
        }
      }

      if (newTotalForecastErrors.Count > 0)
      {
        // Calculating total forecast error from all installations
        var totalForecastError = _forecastErrorsCalculation
          .CalculateTotalForecastError(newTotalForecastErrors, _idForAllInstallations);

        newTotalForecastErrors.Add(totalForecastError);

        // Update total forecast errors
        _databaseHelper.RemoveTotalForecastErrors();
        await _databaseHelper.SaveForecastErrors(newTotalForecastErrors);
      }
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
