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
    private readonly DateTime dateTimeMinValue;
    private readonly short _minNumberOfMeasurements;

    public DatabaseHelper(AirlyContext context, short minNumberOfMeasurements)
    {
      _context = context;
      _minNumberOfMeasurements = minNumberOfMeasurements;
      dateTimeMinValue = new DateTime(2000, 1, 1);
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
      var lastMeasurementDate = dateTimeMinValue;

      var selectedDates = _context.ArchiveMeasurements
        .Where(e => e.InstallationId == installationId)
        .OrderByDescending(e => e.TillDateTime)
        .Select(e => e.TillDateTime);

      // Check if some of measurements there already are in Database
      if (selectedDates.Any())
      {
        lastMeasurementDate = selectedDates
          .First();
      }

      while (newMeasurements.Count > 0 && newMeasurements[0].TillDateTime <= lastMeasurementDate.ToLocalTime())
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
      var lastForecastDate = dateTimeMinValue;

      var selectedDates = _context.ArchiveForecasts
        .Where(e => e.InstallationId == installationId)
        .OrderByDescending(e => e.TillDateTime)
        .Select(e => e.TillDateTime);

      // Check if some of forecasts there already are in Database
      if (selectedDates.Any())
      {
        lastForecastDate = selectedDates
          .First();
      }

      while (newForecasts.Count > 0 && newForecasts[0].TillDateTime <= lastForecastDate.ToLocalTime())
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
      var lastForecastErrorDate = dateTimeMinValue;

      var selectedDates = _context.ForecastErrors
        .Where(e => e.InstallationId == installationId)
        .OrderByDescending(e => e.TillDateTime)
        .Select(e => e.TillDateTime);

      if (selectedDates.Any())
      {
        lastForecastErrorDate = selectedDates
          .First();
      }

      _newArchiveMeasurements = _context.ArchiveMeasurements
        .Where(m => m.InstallationId == installationId
                 && m.TillDateTime > lastForecastErrorDate)
        .ToList();

      _newArchiveForecasts = _context.ArchiveForecasts
        .Where(f => f.InstallationId == installationId
                 && f.TillDateTime > lastForecastErrorDate)
        .ToList();
    }

    public DateTime SelectLastMeasurementDate(short installationId)
    {
      var lastMeasurementDate = dateTimeMinValue;

      var selectedDates = _context.ArchiveMeasurements
          .Where(e => e.InstallationId == installationId)
          .OrderByDescending(e => e.TillDateTime)
          .Select(e => e.TillDateTime);

      if (selectedDates.Any())
      {
        lastMeasurementDate = selectedDates
          .First();
      }

      return lastMeasurementDate;
    }
  }
}
