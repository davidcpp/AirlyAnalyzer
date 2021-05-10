﻿namespace AirlyAnalyzer.Data
{
  using System;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Models.Weather;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  public class UnitOfWork : IDisposable
  {
    private readonly AirlyContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    private GenericRepository<AirQualityMeasurement> measurementRepo;
    private GenericRepository<AirQualityForecast> forecastRepo;
    private ForecastErrorsRepository forecastErrorRepo;
    private PlainForecastErrorsRepository plainForecastErrorRepo;
    private ScaleForecastErrorsRepository scaleForecastErrorRepo;
    private BasicRepository<InstallationInfo> installationsRepo;
    private BasicRepository<WeatherMeasurement> weatherMeasurementsRepo;

    private bool disposedValue;

    public UnitOfWork(
        AirlyContext context,
        ILogger<UnitOfWork> logger = null)
    {
      _context = context;
      _logger = logger;
    }

    public GenericRepository<AirQualityMeasurement> MeasurementRepository
        => measurementRepo
            ??= new GenericRepository<AirQualityMeasurement>(_context);

    public GenericRepository<AirQualityForecast> ForecastRepository
        => forecastRepo
            ??= new GenericRepository<AirQualityForecast>(_context);

    public ForecastErrorsRepository ForecastErrorRepository
        => forecastErrorRepo
            ??= new ForecastErrorsRepository(_context);

    public PlainForecastErrorsRepository PlainForecastErrorRepository
        => plainForecastErrorRepo
            ??= new PlainForecastErrorsRepository(_context);

    public ScaleForecastErrorsRepository ScaleForecastErrorRepository
        => scaleForecastErrorRepo
            ??= new ScaleForecastErrorsRepository(_context);

    public BasicRepository<InstallationInfo> InstallationsRepository
        => installationsRepo
            ??= new BasicRepository<InstallationInfo>(_context);

    public BasicRepository<WeatherMeasurement> WeatherMeasurementsRepository
        => weatherMeasurementsRepo
            ??= new BasicRepository<WeatherMeasurement>(_context);

    public async Task SaveChangesAsync()
    {
      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateException e)
      {
        _logger?.LogError(e.Message);
      }
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
