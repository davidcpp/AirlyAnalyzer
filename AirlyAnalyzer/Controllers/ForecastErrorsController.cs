namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.AspNetCore.Mvc;

  public class ForecastErrorsController : Controller
  {
    private readonly GenericRepository<AirQualityForecastError> _forecastErrorRepo;

    public ForecastErrorsController(
        GenericRepository<AirQualityForecastError> forecastErrorRepo)
    {
      _forecastErrorRepo = forecastErrorRepo;
    }

    // GET: ForecastErrors
    public IActionResult Index()
    {
      var requestDates = _forecastErrorRepo.GetParameters<DateTime>(
          selectPredicate: fe => fe.RequestDateTime.Date,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime),
          isDistinct: true)
        .ToList();

      if (requestDates.Count > 0)
      {
        var selectedRequestDate = requestDates[0];

        var errorsInDay = _forecastErrorRepo.Get(
            wherePredicate: fe => fe.RequestDateTime.Date == selectedRequestDate);

        return View(errorsInDay);
      }

      return View(new List<AirQualityForecastError>());
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _forecastErrorRepo.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
