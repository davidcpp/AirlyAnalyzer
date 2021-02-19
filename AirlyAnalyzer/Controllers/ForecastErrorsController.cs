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
