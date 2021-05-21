namespace AirlyAnalyzer.Data
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

    private AirQualityRepository<AirQualityMeasurement> measurementRepo;
    private AirQualityRepository<AirQualityForecast> forecastRepo;
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

    public AirQualityRepository<AirQualityMeasurement> MeasurementRepository
        => measurementRepo
            ??= new AirQualityRepository<AirQualityMeasurement>(_context);

    public AirQualityRepository<AirQualityForecast> ForecastRepository
        => forecastRepo
            ??= new AirQualityRepository<AirQualityForecast>(_context);

    public ForecastErrorsRepository ForecastErrorRepository
        => forecastErrorRepo
            ??= new ForecastErrorsRepository(_context);

    public ForecastErrorsRepository PlainForecastErrorRepository
        => plainForecastErrorRepo
            ??= new ForecastErrorsRepository(_context);

    public ForecastErrorsRepository ScaleForecastErrorRepository
        => scaleForecastErrorRepo
            ??= new ForecastErrorsRepository(_context);

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
