namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Calculation;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;

  public class ProgramController : IHostedService, IDisposable
  {
    private readonly ForecastErrorsCalculation _forecastErrorsCalculation;
    private readonly AirQualityDataDownloader _airQualityDataDownloader;

    private readonly ILogger<ProgramController> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly List<short> _installationIDsList;
    private readonly short _idForAllInstallations;
    private readonly short _minNumberOfMeasurements;

    private UnitOfWork _unitOfWork;
    private Timer _timer;

    public ProgramController(UnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    public ProgramController(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<ProgramController> logger = null)
    {
      _scopeFactory = scopeFactory;
      _logger = logger;

      _minNumberOfMeasurements = config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _installationIDsList = config.GetSection(
          "AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _idForAllInstallations = config.GetValue<short>(
          "AppSettings:AirlyApi:IdForAllInstallations");

      _airQualityDataDownloader = new AirQualityDataDownloader(config);

      _forecastErrorsCalculation =
          new ForecastErrorsCalculation(_minNumberOfMeasurements);
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
      _logger?.LogInformation("ProgramController is starting");

      _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

      return Task.CompletedTask;
    }

    public async void DoWork(object state)
    {
      using (var scope = _scopeFactory.CreateScope())
      {
        _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        var (newMeasurements, newForecasts) = await DownloadAllAirQualityData();

        if (await SaveAllAirQualityData(newMeasurements, newForecasts) > 0)
        {
          var (hourlyErrors, dailyErrors) = await CalculateForecastErrors();
          await SaveForecastErrors(hourlyErrors, dailyErrors);

          var newTotalForecastErrors = await CalculateTotalForecastErrors();
          await UpdateTotalForecastErrors(newTotalForecastErrors);
        }
      }
    }

    public async Task<(List<AirQualityMeasurement>, List<AirQualityForecast>)>
        DownloadAllAirQualityData()
    {
      _logger?.LogInformation("DownloadAllAirQualityData() is starting");

      var newMeasurements = new List<AirQualityMeasurement>();
      var newForecasts = new List<AirQualityForecast>();

      var requestDateTime = DateTime.UtcNow;

      // Downloading and saving new data in database
      foreach (short installationId in _installationIDsList)
      {
        var lastMeasurementDate = await _unitOfWork
            .MeasurementRepository.GetLastDate(installationId);

        if ((requestDateTime - lastMeasurementDate).TotalHours
            >= _minNumberOfMeasurements)
        {
          var responseMeasurements = await _airQualityDataDownloader
              .DownloadAirQualityData(installationId);

          newMeasurements.AddRange(responseMeasurements.History
              .ConvertToAirQualityMeasurements(installationId, requestDateTime));

          newForecasts.AddRange(responseMeasurements.Forecast
              .ConvertToAirQualityForecasts(installationId, requestDateTime));
        }
      }

      return (newMeasurements, newForecasts);
    }

    public async Task<int> SaveAllAirQualityData(
        List<AirQualityMeasurement> newMeasurements,
        List<AirQualityForecast> newForecasts)
    {
      _logger?.LogInformation("SaveAllAirQualityData() is starting");

      await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
      await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      return Math.Min(newMeasurements.Count, newForecasts.Count);
    }

    public async Task<(List<AirQualityForecastError>, List<AirQualityForecastError>)>
        CalculateForecastErrors()
    {
      _logger?.LogInformation("CalculateForecastErrors() is starting");

      var allHourlyForecastErrors = new List<AirQualityForecastError>();
      var allDailyForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIDsList)
      {
        var (newArchiveMeasurements, newArchiveForecasts) = await _unitOfWork
            .ForecastErrorRepository.SelectDataToProcessing(installationId);

        var hourlyForecastErrors = _forecastErrorsCalculation
            .CalculateHourlyForecastErrors(
                installationId, newArchiveMeasurements, newArchiveForecasts);

        allHourlyForecastErrors.AddRange(hourlyForecastErrors);

        allDailyForecastErrors.AddRange(_forecastErrorsCalculation
            .CalculateDailyForecastErrors(installationId, hourlyForecastErrors));
      }

      return (allHourlyForecastErrors, allDailyForecastErrors);
    }

    public async Task<int> SaveForecastErrors(
        List<AirQualityForecastError> hourlyForecastErrors,
        List<AirQualityForecastError> dailyForecastErrors)
    {
      _logger?.LogInformation("SaveForecastErrors() is starting");

      await _unitOfWork.ForecastErrorRepository.AddAsync(hourlyForecastErrors);
      await _unitOfWork.ForecastErrorRepository.AddAsync(dailyForecastErrors);
      await _unitOfWork.SaveChangesAsync();

      return dailyForecastErrors.Count;
    }

    public async Task<List<AirQualityForecastError>> CalculateTotalForecastErrors()
    {
      _logger?.LogInformation("CalculateTotalForecastErrors() is starting");

      var newTotalForecastErrors = new List<AirQualityForecastError>();

      // Calculating total forecast errors for each installation
      foreach (short installationId in _installationIDsList)
      {
        var dailyForecastErrors = await _unitOfWork.ForecastErrorRepository.Get(
            fe => fe.InstallationId == installationId
               && fe.ErrorType == ForecastErrorType.Daily);

        if (dailyForecastErrors.Count > 0)
        {
          var installationForecastError = _forecastErrorsCalculation
              .CalculateTotalForecastError(installationId, dailyForecastErrors);

          newTotalForecastErrors.Add(installationForecastError);
        }
      }

      if (newTotalForecastErrors.Count > 0)
      {
        // Calculating total forecast error from all installations
        var totalForecastError =
            _forecastErrorsCalculation.CalculateTotalForecastError(
                _idForAllInstallations, newTotalForecastErrors);

        newTotalForecastErrors.Add(totalForecastError);
      }

      return newTotalForecastErrors;
    }

    public async Task UpdateTotalForecastErrors(
        List<AirQualityForecastError> newTotalForecastErrors)
    {
      _logger?.LogInformation("UpdateTotalForecastErrors() is starting");

      if (newTotalForecastErrors.Count > 0)
      {
        _unitOfWork.ForecastErrorRepository.Delete(
            fe => fe.ErrorType == ForecastErrorType.Total);

        await _unitOfWork.SaveChangesAsync();

        await _unitOfWork.ForecastErrorRepository.AddAsync(newTotalForecastErrors);
        await _unitOfWork.SaveChangesAsync();
      }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
      _logger?.LogInformation("ProgramController is stopping");

      _timer?.Change(Timeout.Infinite, 0);

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }
  }
}