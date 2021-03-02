﻿namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;

  public class AirlyAnalyzerRepository : IDisposable
  {
    private readonly AirlyContext _context;
    private readonly GenericRepository<AirQualityMeasurement> _measurementRepo;
    private readonly GenericRepository<AirQualityForecast> _forecastRepo;
    private readonly GenericRepository<AirQualityForecastError> _forecastErrorRepo;

    private readonly DateTime _dateTimeMinValue = new DateTime(2000, 1, 1);

    private bool disposedValue;

    public AirlyAnalyzerRepository(
        AirlyContext context,
        GenericRepository<AirQualityMeasurement> measurementRepo = null,
        GenericRepository<AirQualityForecast> forecastRepo = null,
        GenericRepository<AirQualityForecastError> forecastErrorRepo = null)
    {
      _context = context;
      _measurementRepo = measurementRepo;
      _forecastRepo = forecastRepo;
      _forecastErrorRepo = forecastErrorRepo;
    }

    public async Task SaveNewMeasurements(
        short installationId,
        short minNumberOfMeasurements,
        List<AirQualityMeasurement> newMeasurements)
    {
      if (_measurementRepo == null)
      {
        return;
      }

      var lastMeasurementDate = _dateTimeMinValue;

      var selectedDates = _measurementRepo
          .GetParameters<DateTime>(
              wherePredicate: m => m.InstallationId == installationId,
              selectPredicate: f => f.TillDateTime,
              orderByMethod: q => q.OrderByDescending(dateTime => dateTime));

      // Check if some of measurements there already are in Database
      if (selectedDates.AsQueryable().Any())
      {
        lastMeasurementDate = await selectedDates.AsQueryable()
            .FirstAsync();
      }

      while (newMeasurements.Count > 0
          && newMeasurements[0].TillDateTime <= lastMeasurementDate.ToLocalTime())
      {
        newMeasurements.RemoveAt(0);
      }

      if (newMeasurements.Count >= minNumberOfMeasurements)
      {
        await _context.ArchiveMeasurements.AddRangeAsync(newMeasurements);
        await _context.SaveChangesAsync();
      }
    }

    public async Task SaveNewForecasts(
        short installationId,
        short minNumberOfMeasurements,
        List<AirQualityForecast> newForecasts)
    {
      if (_forecastRepo == null)
      {
        return;
      }

      var lastForecastDate = _dateTimeMinValue;

      var selectedDates = _forecastRepo
          .GetParameters<DateTime>(
              wherePredicate: f => f.InstallationId == installationId,
              selectPredicate: f => f.TillDateTime,
              orderByMethod: q => q.OrderByDescending(dateTime => dateTime));

      // Check if some of forecasts there already are in Database
      if (selectedDates.AsQueryable().Any())
      {
        lastForecastDate = await selectedDates.AsQueryable()
            .FirstAsync();
      }

      while (newForecasts.Count > 0
          && newForecasts[0].TillDateTime <= lastForecastDate.ToLocalTime())
      {
        newForecasts.RemoveAt(0);
      }

      if (newForecasts.Count >= minNumberOfMeasurements)
      {
        await _context.ArchiveForecasts.AddRangeAsync(newForecasts);
        await _context.SaveChangesAsync();
      }
    }

    public async Task<(List<AirQualityMeasurement>, List<AirQualityForecast>)>
        SelectDataToProcessing(short installationId)
    {
      if (_forecastErrorRepo == null)
      {
        return (new List<AirQualityMeasurement>(), new List<AirQualityForecast>());
      }

      var lastForecastErrorDate = _dateTimeMinValue;

      var selectedDates = _forecastErrorRepo.GetParameters<DateTime>(
          wherePredicate: fe => fe.InstallationId == installationId,
          selectPredicate: fe => fe.TillDateTime,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime));

      if (selectedDates.AsQueryable().Any())
      {
        lastForecastErrorDate = await selectedDates.AsQueryable()
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

      var selectedDates = _measurementRepo?.GetParameters<DateTime>(
          wherePredicate: m => m.InstallationId == installationId,
          selectPredicate: m => m.TillDateTime,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime));

      if (selectedDates?.AsQueryable().Any() == true)
      {
        lastMeasurementDate = await selectedDates.AsQueryable()
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
