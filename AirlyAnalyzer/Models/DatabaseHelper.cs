namespace AirlyAnalyzer.Models
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;

  public class DatabaseHelper
  {
    private readonly AirlyContext _context;
    private readonly short _minNumberOfMeasurements;

    public DatabaseHelper(AirlyContext context, short minNumberOfMeasurements)
    {
      _context = context;
      _minNumberOfMeasurements = minNumberOfMeasurements;
    }

    public void RemoveTotalForecastErrors()
    {
      _context.ForecastErrors.RemoveRange(
        _context.ForecastErrors.Where(e => e.ErrorType == ForecastErrorType.Total));
    }

    public async Task<int> SaveForecastErrors(List<AirQualityForecastError> forecastErrors)
    {
      _context.ForecastErrors.AddRange(forecastErrors);
      return await _context.SaveChangesAsync();
    }

    public async Task SaveNewMeasurements(List<AirQualityMeasurement> newMeasurements, short installationId)
    {
      var lastMeasurementDate = DateTime.MinValue;

      // Check if some of measurements there already are in Database
      if (_context.ArchiveMeasurements.Any())
      {
        lastMeasurementDate = _context.ArchiveMeasurements
          .Where(e => e.InstallationId == installationId)
          .OrderByDescending(e => e.FromDateTime)
          .Select(e => e.FromDateTime)
          .First()
          .ToLocalTime();
      }

      while (newMeasurements.Count > 0 && newMeasurements[0].FromDateTime <= lastMeasurementDate)
      {
        newMeasurements.RemoveAt(0);
      }

      if (newMeasurements.Count >= _minNumberOfMeasurements)
      {
        _context.ArchiveMeasurements.AddRange(newMeasurements);
        await _context.SaveChangesAsync();
      }
    }

    public async Task SaveNewForecasts(List<AirQualityForecast> newForecasts, short installationId)
    {
      var lastForecastDate = DateTime.MinValue;

      // Check if some of forecasts there already are in Database
      if (_context.ArchiveForecasts.Any())
      {
        lastForecastDate = _context.ArchiveForecasts
          .Where(e => e.InstallationId == installationId)
          .OrderByDescending(e => e.FromDateTime)
          .Select(e => e.FromDateTime)
          .First()
          .ToLocalTime();
      }

      while (newForecasts.Count > 0 && newForecasts[0].FromDateTime <= lastForecastDate)
      {
        newForecasts.RemoveAt(0);
      }

      if (newForecasts.Count >= _minNumberOfMeasurements)
      {
        _context.ArchiveForecasts.AddRange(newForecasts);
        await _context.SaveChangesAsync();
      }
    }

    public List<AirQualityForecastError> SelectDailyForecastErrors(short installationId)
    {
      return _context.ForecastErrors
        .Where(e => e.InstallationId == installationId && e.ErrorType == ForecastErrorType.Daily)
        .ToList();
    }

    public void SelectDataToProcessing(
      short installationId,
      out List<AirQualityMeasurement> _newArchiveMeasurements,
      out List<AirQualityForecast> _newArchiveForecasts)
    {
      var lastForecastErrorDate = DateTime.MinValue;

      if (_context.ForecastErrors.Any())
      {
        lastForecastErrorDate = _context.ForecastErrors
          .Where(e => e.InstallationId == installationId)
          .OrderByDescending(e => e.TillDateTime)
          .Select(e => e.TillDateTime)
          .First();
      }

      _newArchiveMeasurements = _context.ArchiveMeasurements
        .Where(m => m.InstallationId == installationId
                 && m.TillDateTime > lastForecastErrorDate)
        .ToList();

      if (_newArchiveMeasurements.Count == 0)
      {
        _newArchiveForecasts = new List<AirQualityForecast>();
      }
      else
      {
        var lastMeasurementDate = _newArchiveMeasurements.Last().TillDateTime.ToUniversalTime();

        _newArchiveForecasts = _context.ArchiveForecasts
          .Where(f => f.InstallationId == installationId
                   && f.TillDateTime > lastForecastErrorDate
                   && f.TillDateTime <= lastMeasurementDate)
          .ToList();
      }
    }

    public DateTime SelectLastMeasurementDate(short installationId)
    {
      var lastMeasurementDate = DateTime.MinValue;

      if (_context.ArchiveMeasurements.Any())
      {
        lastMeasurementDate = _context.ArchiveMeasurements
          .Where(e => e.InstallationId == installationId)
          .OrderByDescending(e => e.FromDateTime)
          .Select(e => e.TillDateTime)
          .First();
      }

      return lastMeasurementDate;
    }
  }
}
