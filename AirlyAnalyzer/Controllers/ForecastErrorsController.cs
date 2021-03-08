namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
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
    public async Task<ActionResult<List<AirQualityForecastError>>> Index()
    {
      var requestDates = await _unitOfWork
          .ForecastErrorRepository.GetParameters<DateTime>(
              selectPredicate: fe => fe.RequestDateTime.Date,
              orderByMethod: q => q.OrderByDescending(dateTime => dateTime),
              isDistinct: true);

      if (requestDates.Count > 0)
      {
        var selectedRequestDate = requestDates[0];

        var errorsInDay = await _unitOfWork.ForecastErrorRepository.Get(
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
