namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
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
    public ActionResult<IEnumerable<AirQualityForecastError>>
        GetErrorsInDay(DateTime selectedRequestDate)
    {
      var errorsInDay = _unitOfWork.ForecastErrorRepository.Get(
          wherePredicate: fe => fe.RequestDateTime.Date == selectedRequestDate)
              .ToList();

      if (errorsInDay.Count == 0)
      {
        return NotFound();
      }

      return errorsInDay;
    }

    // GET: api/<ForecastErrorsApiController>/GetRequestDates
    [HttpGet]
    public IEnumerable<DateTime> GetRequestDates()
    {
      return _unitOfWork.ForecastErrorRepository.GetParameters<DateTime>(
          selectPredicate: fe => fe.RequestDateTime.Date,
          orderByMethod: q => q.OrderBy(dateTime => dateTime),
          isDistinct: true);
    }
  }
}
