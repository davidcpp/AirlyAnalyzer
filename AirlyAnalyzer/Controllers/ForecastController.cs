namespace AirlyAnalyzer.Controllers
{
  using System.Linq;
  using AirlyAnalyzer.Data;
  using Microsoft.AspNetCore.Mvc;

  public class ForecastController : Controller
  {
    private readonly AirlyContext _context;

    public ForecastController(AirlyContext context)
    {
      _context = context;
    }

    // GET: Forecast
    public IActionResult Index()
    {
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
