namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Configuration;

  [Route("api/[controller]/[action]")]
  [ApiController]
  public class ForecastErrorsApiController : ControllerBase
  {
    private DatabaseHelper _databaseHelper;

    public ForecastErrorsApiController(AirlyContext context, IConfiguration config)
    {
      short minNumberOfMeasurements = config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _databaseHelper = new DatabaseHelper(context, minNumberOfMeasurements);
    }

    // GET: api/<ForecastErrorsApiController>/GetErrorsInDay/{day}
    [HttpGet("{day}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<AirQualityForecastError>>
        GetErrorsInDay(int day)
    {
      var requestDates = _databaseHelper
          .GetParameters<AirQualityForecastError, DateTime>(
              selectPredicate: fe => fe.RequestDateTime.Date,
              orderByMethod: q => q.OrderBy(dateTime => dateTime),
              isDistinct: true)
          .ToList();

      if (day < 1 || day > requestDates.Count)
      {
        return NotFound();
      }

      var selectedRequestDate = requestDates[day - 1];

      var errorsInDay = _databaseHelper.Get<AirQualityForecastError>(
          wherePredicate: fe => fe.RequestDateTime.Date == selectedRequestDate);

      return new ActionResult<IEnumerable<AirQualityForecastError>>(errorsInDay);
    }

    // GET: api/<ForecastErrorsApiController>/GetNumberOfDays
    [HttpGet]
    public int GetNumberOfDays()
    {
      var requestDates = _databaseHelper
          .GetParameters<AirQualityForecastError, DateTime>(
              selectPredicate: fe => fe.RequestDateTime.Date,
              isDistinct: true);

      return requestDates.Count();
    }

    // GET: api/<ForecastErrorsApiController>/GetRequestDates
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IEnumerable<DateTime> GetRequestDates()
    {
      return _databaseHelper
          .GetParameters<AirQualityForecastError, DateTime>(
              selectPredicate: fe => fe.RequestDateTime.Date,
              orderByMethod: q => q.OrderBy(dateTime => dateTime),
              isDistinct: true);
    }
  }
}
