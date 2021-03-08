namespace AirlyAnalyzer.Data
{
  using System;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;

  public class UnitOfWork : IDisposable
  {
    private readonly AirlyContext _context;

    private GenericRepository<AirQualityMeasurement> measurementRepo;
    private GenericRepository<AirQualityForecast> forecastRepo;
    private GenericRepository<AirQualityForecastError> forecastErrorRepo;
    private ForecastErrorsRepository airlyAnalyzerRepo;

    private bool disposedValue;

    public UnitOfWork(AirlyContext context)
    {
      _context = context;
    }

    public GenericRepository<AirQualityMeasurement> MeasurementRepository
        => measurementRepo
            ??= new GenericRepository<AirQualityMeasurement>(_context);

    public GenericRepository<AirQualityForecast> ForecastRepository
        => forecastRepo
            ??= new GenericRepository<AirQualityForecast>(_context);

    public GenericRepository<AirQualityForecastError> ForecastErrorRepository
        => forecastErrorRepo
            ??= new GenericRepository<AirQualityForecastError>(_context);

    public ForecastErrorsRepository AirlyAnalyzerRepository
        => airlyAnalyzerRepo
            ??= new ForecastErrorsRepository(_context);

    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
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
