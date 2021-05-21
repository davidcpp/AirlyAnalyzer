﻿namespace AirlyAnalyzer.Data
{
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

    public virtual async Task<(List<AirQualityMeasurement>, List<AirQualityForecast>)>
        SelectDataToProcessing(short installationId)
    {
      var lastForecastErrorDate = await GetLastDate(installationId);

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
