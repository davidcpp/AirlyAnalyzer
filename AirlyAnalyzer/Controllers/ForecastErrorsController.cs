namespace AirlyAnalyzer.Controllers
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  public class ForecastErrorsController : Controller
  {
    private readonly UnitOfWork _unitOfWork;
    private readonly ILogger<ForecastErrorsController> _logger;

    public ForecastErrorsController(
        UnitOfWork unitOfWork,
        ILogger<ForecastErrorsController> logger = null)
    {
      _unitOfWork = unitOfWork;
      _logger = logger;
    }

    // GET: ForecastErrors
    public async Task<ActionResult> Index()
    {
      _logger?.LogInformation("GET: ForecastErrors/Index");

      var requestDates = await _unitOfWork
          .ForecastErrorRepository.GetParameters(
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
