﻿namespace AirlyAnalyzer.Data
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;

  public class ScaleForecastErrorsRepository : ForecastErrorsRepository
  {
    public ScaleForecastErrorsRepository(AirlyContext context) : base(context)
    {
    }

    public override async Task<DateTime> GetLastDate(short installationId)
    {
      var lastDate = _dateTimeMinValue;

      var selectedDates = await GetParameters(
          wherePredicate:
              m => m.InstallationId == installationId
                && m.Class == ForecastErrorClass.Scale,
          selectPredicate: m => m.TillDateTime,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime));

      if (selectedDates.Count > 0)
      {
        lastDate = selectedDates[0];
      }

      return lastDate;
    }
  }
}
