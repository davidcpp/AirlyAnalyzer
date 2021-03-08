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
    private readonly UnitOfWork _unitOfWork;

    public ForecastErrorsController(UnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    // GET: ForecastErrors
    public IActionResult Index()
    {
      var requestDates = _unitOfWork
          .ForecastErrorRepository.GetParameters<DateTime>(
              selectPredicate: fe => fe.RequestDateTime.Date,
              orderByMethod: q => q.OrderByDescending(dateTime => dateTime),
              isDistinct: true)
          .ToList();

      if (requestDates.Count > 0)
      {
        var selectedRequestDate = requestDates[0];

        var errorsInDay = _unitOfWork.ForecastErrorRepository.Get(
            wherePredicate: fe => fe.RequestDateTime.Date == selectedRequestDate);

        return View(errorsInDay);
      }

      return View(new List<AirQualityForecastError>());
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
