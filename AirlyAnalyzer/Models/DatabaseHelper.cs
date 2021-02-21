namespace AirlyAnalyzer.Models
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using Microsoft.EntityFrameworkCore;

  public class DatabaseHelper : IDisposable
  {
    private readonly AirlyContext _context;
    private readonly DateTime _dateTimeMinValue;
    private readonly short _minNumberOfMeasurements;

    private bool disposedValue;

    public DatabaseHelper(AirlyContext context, short minNumberOfMeasurements)
    {
      _context = context;
      _dateTimeMinValue = new DateTime(2000, 1, 1);
      _minNumberOfMeasurements = minNumberOfMeasurements;
    }

    public IEnumerable<TEntity> Get<TEntity>(
        Expression<Func<TEntity, bool>> wherePredicate = null)
            where TEntity : class
    {
      IQueryable<TEntity> query = _context.Set<TEntity>();

      if (wherePredicate != null)
      {
        query = query.Where(wherePredicate);
      }

      return query;
    }

    public IEnumerable<T> GetParameters<TEntity, T>(
        Expression<Func<TEntity, T>> selectPredicate,
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByMethod = null,
        bool isDistinct = false)
            where TEntity : class
    {
      IQueryable<TEntity> query = _context.Set<TEntity>();

      if (wherePredicate != null)
      {
        query = query.Where(wherePredicate);
      }

      IQueryable<T> resultQuery;
      if (selectPredicate != null)
      {
        resultQuery = query.Select(selectPredicate);

        if (isDistinct)
        {
          resultQuery = resultQuery.Distinct();
        }

        if (orderByMethod != null)
        {
          resultQuery = orderByMethod(resultQuery);
        }

        return resultQuery;
      }
      else
      {
        return new List<T>();
      }
    }

    public void RemoveTotalForecastErrors()
    {
      _context.ForecastErrors.RemoveRange(
          _context.ForecastErrors.Where(
              fe => fe.ErrorType == ForecastErrorType.Total));
    }

    public async Task<int> SaveForecastErrors(
        List<AirQualityForecastError> forecastErrors)
    {
      _context.ForecastErrors.AddRange(forecastErrors);
      return await _context.SaveChangesAsync();
    }

    public async Task SaveNewMeasurements(
        short installationId,
        List<AirQualityMeasurement> newMeasurements)
    {
      var lastMeasurementDate = _dateTimeMinValue;

      var selectedDates = _context.ArchiveMeasurements
          .Where(m => m.InstallationId == installationId)
          .OrderByDescending(m => m.TillDateTime)
          .Select(m => m.TillDateTime);

      // Check if some of measurements there already are in Database
      if (selectedDates.Any())
      {
        lastMeasurementDate = await selectedDates
            .FirstAsync();
      }

      while (newMeasurements.Count > 0
          && newMeasurements[0].TillDateTime <= lastMeasurementDate.ToLocalTime())
      {
        newMeasurements.RemoveAt(0);
      }

      if (newMeasurements.Count >= _minNumberOfMeasurements)
      {
        _context.ArchiveMeasurements.AddRange(newMeasurements);
        await _context.SaveChangesAsync();
      }
    }

    public async Task SaveNewForecasts(
        short installationId,
        List<AirQualityForecast> newForecasts)
    {
      var lastForecastDate = _dateTimeMinValue;

      var selectedDates = _context.ArchiveForecasts
          .Where(f => f.InstallationId == installationId)
          .OrderByDescending(f => f.TillDateTime)
          .Select(f => f.TillDateTime);

      // Check if some of forecasts there already are in Database
      if (selectedDates.Any())
      {
        lastForecastDate = await selectedDates
            .FirstAsync();
      }

      while (newForecasts.Count > 0
          && newForecasts[0].TillDateTime <= lastForecastDate.ToLocalTime())
      {
        newForecasts.RemoveAt(0);
      }

      if (newForecasts.Count >= _minNumberOfMeasurements)
      {
        _context.ArchiveForecasts.AddRange(newForecasts);
        await _context.SaveChangesAsync();
      }
    }

    public Task<List<AirQualityForecastError>>
        SelectDailyForecastErrors(short installationId)
    {
      return _context.ForecastErrors
          .Where(fe => fe.InstallationId == installationId
                   && fe.ErrorType == ForecastErrorType.Daily)
          .ToListAsync();
    }

    public async Task<(List<AirQualityMeasurement>, List<AirQualityForecast>)>
        SelectDataToProcessing(short installationId)
    {
      var lastForecastErrorDate = _dateTimeMinValue;

      var selectedDates = _context.ForecastErrors
          .Where(fe => fe.InstallationId == installationId)
          .OrderByDescending(fe => fe.TillDateTime)
          .Select(fe => fe.TillDateTime);

      if (selectedDates.Any())
      {
        lastForecastErrorDate = await selectedDates
            .FirstAsync();
      }

      var _newArchiveMeasurements = await _context.ArchiveMeasurements
          .Where(m => m.InstallationId == installationId
                   && m.TillDateTime > lastForecastErrorDate)
          .ToListAsync();

      var _newArchiveForecasts = await _context.ArchiveForecasts
          .Where(f => f.InstallationId == installationId
                   && f.TillDateTime > lastForecastErrorDate)
          .ToListAsync();

      return (_newArchiveMeasurements, _newArchiveForecasts);
    }

    public async Task<DateTime> SelectLastMeasurementDate(short installationId)
    {
      var lastMeasurementDate = _dateTimeMinValue;

      var selectedDates = _context.ArchiveMeasurements
          .Where(m => m.InstallationId == installationId)
          .OrderByDescending(m => m.TillDateTime)
          .Select(m => m.TillDateTime);

      if (selectedDates.Any())
      {
        lastMeasurementDate = await selectedDates
            .FirstAsync();
      }

      return lastMeasurementDate;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _context.Dispose();
        }
        disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
