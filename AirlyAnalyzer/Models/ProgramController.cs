namespace AirlyAnalyzer.Models
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
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

    private DatabaseHelper _databaseHelper;
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
        var context = scope.ServiceProvider.GetRequiredService<AirlyContext>();
        _databaseHelper = new DatabaseHelper(context, _minNumberOfMeasurements);

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
        var lastMeasurementDate =
            await _databaseHelper.SelectLastMeasurementDate(installationId);

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

          await _databaseHelper
              .SaveNewMeasurements(installationId, newMeasurements);

          await _databaseHelper
              .SaveNewForecasts(installationId, newForecasts);
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
        var (newArchiveMeasurements, newArchiveForecasts) =
            await _databaseHelper.SelectDataToProcessing(installationId);

        var hourlyForecastErrors = 
            _forecastErrorsCalculation.CalculateHourlyForecastErrors(
                installationId, newArchiveMeasurements, newArchiveForecasts);

        await _databaseHelper.SaveForecastErrors(hourlyForecastErrors);

        var dailyForecastErrors =
            _forecastErrorsCalculation.CalculateDailyForecastErrors(
                installationId, hourlyForecastErrors);

        await _databaseHelper.SaveForecastErrors(dailyForecastErrors);
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
        var dailyForecastErrors =
            await _databaseHelper.SelectDailyForecastErrors(installationId);

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
        _databaseHelper.RemoveTotalForecastErrors();
        await _databaseHelper.SaveForecastErrors(newTotalForecastErrors);
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