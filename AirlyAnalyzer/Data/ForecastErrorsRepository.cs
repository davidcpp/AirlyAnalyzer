namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;

  public class ForecastErrorsRepository : AirQualityRepository<AirQualityForecastError>
  {
    public ForecastErrorsRepository(AirlyContext context) : base(context)
    {
    }

    public async Task<DateTime> GetLastDate(
        short installationId,
        ForecastErrorClass errorClass,
        AirQualityDataSource source = AirQualityDataSource.Airly)
    {
      var lastDate = _dateTimeMinValue;

      var selectedDates = await GetParameters(
          wherePredicate:
              m => m.InstallationId == installationId
                && m.Class == errorClass
                && m.Source == source,
          selectPredicate: m => m.TillDateTime,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime));

      if (selectedDates.Count > 0)
      {
        lastDate = selectedDates[0];
      }

      return lastDate;
    }

    public virtual async Task<(List<AirQualityMeasurement>, List<AirQualityForecast>)>
        SelectDataToProcessing(short installationId, ForecastErrorClass errorClass)
    {
      var lastForecastErrorDate = await GetLastDate(installationId, errorClass);

      var _newMeasurements = await _context.Set<AirQualityMeasurement>()
          .Where(m => m.InstallationId == installationId
                   && m.TillDateTime > lastForecastErrorDate)
          .ToListAsync();

      var _newForecasts = await _context.Set<AirQualityForecast>()
          .Where(f => f.InstallationId == installationId
                   && f.TillDateTime > lastForecastErrorDate)
          .ToListAsync();

      return (_newMeasurements, _newForecasts);
    }
  }
}
