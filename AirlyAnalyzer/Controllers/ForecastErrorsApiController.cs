namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/[controller]/[action]")]
  [ApiController]
  public class ForecastErrorsApiController : ControllerBase
  {
    private readonly UnitOfWork _unitOfWork;

    public ForecastErrorsApiController(UnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    // GET: api/<ForecastErrorsApiController>/GetErrorsInDay/{day}
    [HttpGet("{selectedRequestDate}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetErrorsInDay(DateTime selectedRequestDate)
    {
      var errorsInDay = await _unitOfWork.ForecastErrorRepository.Get(
          wherePredicate: fe => fe.RequestDateTime.Date == selectedRequestDate);

      if (errorsInDay.Count == 0)
      {
        return NotFound();
      }

      return new ObjectResult(errorsInDay);
    }

    // GET: api/<ForecastErrorsApiController>/GetRequestDates
    [HttpGet]
    public Task<List<DateTime>> GetRequestDates()
    {
      return _unitOfWork.ForecastErrorRepository.GetParameters(
          selectPredicate: fe => fe.RequestDateTime.Date,
          orderByMethod: q => q.OrderBy(dateTime => dateTime),
          isDistinct: true);
    }
  }
}
