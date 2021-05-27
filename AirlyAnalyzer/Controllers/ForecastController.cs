namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;

  public class ForecastController : Controller
  {
    private readonly UnitOfWork _unitOfWork;
    private readonly ILogger<ForecastController> _logger;

    private readonly short _numberOfHours;
    private readonly List<short> _installationIds;

    public ForecastController(
        UnitOfWork unitOfWork,
        IConfiguration config,
        ILogger<ForecastController> logger = null)
    {
      _unitOfWork = unitOfWork;
      _logger = logger;

      _numberOfHours = config.GetValue<short>(
          "AppSettings:AirQualityForecast:ForecastHoursNumber");

      _installationIds = config.GetSection(
          "AppSettings:AirlyApi:InstallationIds").Get<List<short>>();
    }

    public async Task<ActionResult> Index()
    {
      _logger?.LogInformation("GET: Forecast/Index");

      var currentForecasts = new List<AirQualityForecast>();

      foreach (short installationId in _installationIds)
      {
        var currentForecast = await _unitOfWork
            .ForecastRepository.GetLastElements(
                _numberOfHours,
                wherePredicate:
                    f => f.InstallationId == installationId
                      && f.Source == AirQualityDataSource.App
                      && f.TillDateTime > DateTime.UtcNow);

        currentForecasts.AddRange(currentForecast);
      }

      return View(currentForecasts);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _unitOfWork.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
