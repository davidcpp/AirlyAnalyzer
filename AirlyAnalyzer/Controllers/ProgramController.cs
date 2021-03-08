namespace AirlyAnalyzer.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
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

    public ProgramController(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<ProgramController> logger)
    {
      _scopeFactory = scopeFactory;
      _logger = logger;

      _minNumberOfMeasurements = config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _installationIDsList = config.GetSection(
          "AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _idForAllInstallations = config.GetValue<short>(
          "AppSettings:AirlyApi:IdForAllInstallations");

      _airQualityDataDownloader =
          new AirQualityDataDownloader(config);

      _forecastErrorsCalculation =
          new ForecastErrorsCalculation(_minNumberOfMeasurements);
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("ProgramController is starting");

      _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

      return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
      using (var scope = _scopeFactory.CreateScope())
      {
        _unitOfWork = scope.ServiceProvider
            .GetRequiredService<UnitOfWork>();

        if (await DownloadAndSaveAirQualityData() > 0)
        {
          await CalculateAndSaveForecastErrors();
          await CalculateAndSaveTotalForecastErrors();
        }
      }
    }

    private async Task<int> DownloadAndSaveAirQualityData()
    {
      _logger.LogInformation("DownloadAndSaveAirQualityData() is starting");

      int newMeasurementsCount = 0;
      int newForecastsCount = 0;

      var requestDateTime = DateTime.UtcNow;

      // Downloading and saving new data in database
      foreach (short installationId in _installationIDsList)
      {
        var lastMeasurementDate
            = _unitOfWork.MeasurementRepository.GetLastDate(installationId);

        if ((requestDateTime - lastMeasurementDate).TotalHours
            >= _minNumberOfMeasurements)
        {
          var responseMeasurements = await _airQualityDataDownloader
              .DownloadAirQualityData(installationId);

          var newMeasurements = responseMeasurements.History
              .ConvertToAirQualityMeasurements(installationId, requestDateTime);

          var newForecasts = responseMeasurements.Forecast
              .ConvertToAirQualityForecasts(installationId, requestDateTime);

          newMeasurementsCount += newMeasurements.Count;
          newForecastsCount += newForecasts.Count;

          await _unitOfWork.MeasurementRepository.AddAsync(newMeasurements);
          await _unitOfWork.SaveChangesAsync();

          await _unitOfWork.ForecastRepository.AddAsync(newForecasts);
          await _unitOfWork.SaveChangesAsync();
        }
      }

      return Math.Min(newMeasurementsCount, newForecastsCount);
    }

    private async Task CalculateAndSaveForecastErrors()
    {
      _logger.LogInformation("CalculateAndSaveForecastErrors() is starting");

      // Calculating and saving new daily and hourly forecast errors in database
      foreach (short installationId in _installationIDsList)
      {
        var (newArchiveMeasurements, newArchiveForecasts) = await _unitOfWork
            .AirlyAnalyzerRepository.SelectDataToProcessing(installationId);

        var hourlyForecastErrors =
            _forecastErrorsCalculation.CalculateHourlyForecastErrors(
                installationId, newArchiveMeasurements, newArchiveForecasts);

        await _unitOfWork.ForecastErrorRepository.AddAsync(hourlyForecastErrors);

        var dailyForecastErrors =
            _forecastErrorsCalculation.CalculateDailyForecastErrors(
                installationId, hourlyForecastErrors);

        await _unitOfWork.ForecastErrorRepository.AddAsync(dailyForecastErrors);
        await _unitOfWork.SaveChangesAsync();
      }
    }

    private async Task CalculateAndSaveTotalForecastErrors()
    {
      _logger.LogInformation(
          "CalculateAndSaveTotalForecastErrors() is starting");

      var newTotalForecastErrors = new List<AirQualityForecastError>();

      // Calculating total forecast errors for each installation
      foreach (short installationId in _installationIDsList)
      {
        var dailyForecastErrors = _unitOfWork.ForecastErrorRepository.Get(
            fe => fe.InstallationId == installationId
               && fe.ErrorType == ForecastErrorType.Daily).ToList();

        if (dailyForecastErrors.Count > 0)
        {
          var installationForecastError =
              _forecastErrorsCalculation.CalculateTotalForecastError(
                  installationId, dailyForecastErrors);

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

        // Update total forecast errors
        _unitOfWork.ForecastErrorRepository.Delete(
            fe => fe.ErrorType == ForecastErrorType.Total);

        await _unitOfWork.SaveChangesAsync();

        await _unitOfWork.ForecastErrorRepository.AddAsync(newTotalForecastErrors);
        await _unitOfWork.SaveChangesAsync();
      }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("ProgramController is stopping");

      _timer?.Change(Timeout.Infinite, 0);

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }
  }
}