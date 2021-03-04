namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;

  public class ForecastErrorsController : Controller
  {
    private DatabaseHelper _databaseHelper;

    public ForecastErrorsController(AirlyContext context, IConfiguration config)
    {
      short minNumberOfMeasurements = config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _databaseHelper = new DatabaseHelper(context, minNumberOfMeasurements);
    }

    // GET: ForecastErrors
    public IActionResult Index()
    {
      var requestDates = _databaseHelper
          .GetParameters<AirQualityForecastError, DateTime>(
              selectPredicate: fe => fe.RequestDateTime.Date,
              orderByMethod: q => q.OrderByDescending(dateTime => dateTime),
              isDistinct: true);

      if (requestDates.Any())
      {
        var selectedRequestDate = requestDates.First();

        var errorsInDay = _databaseHelper.Get<AirQualityForecastError>(
            wherePredicate: fe => fe.RequestDateTime.Date == selectedRequestDate);

        return View(errorsInDay);
      }

      return View(new List<AirQualityForecastError>());
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _databaseHelper.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
