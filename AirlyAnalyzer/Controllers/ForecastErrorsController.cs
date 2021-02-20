namespace AirlyAnalyzer.Controllers
{
  using System.Linq;
  using AirlyAnalyzer.Data;
  using Microsoft.AspNetCore.Mvc;

  public class ForecastErrorsController : Controller
  {
    private readonly AirlyContext _context;

    public ForecastErrorsController(AirlyContext context)
    {
      _context = context;
    }

    // GET: ForecastErrors
    public IActionResult Index()
    {
      var selectedRequestDate
          = _context.ForecastErrors
              .Select(e => e.RequestDateTime)
              .OrderByDescending(dateTime => dateTime)
              .First()
              .Date;

      var selectedDay = _context.ForecastErrors
          .Where(e => e.RequestDateTime.Date == selectedRequestDate);

      return View(selectedDay);
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
