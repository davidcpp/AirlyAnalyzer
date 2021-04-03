namespace AirlyAnalyzer.Services
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

  using IAirlyMeasurementsDownloader
      = AirlyAnalyzer.Client.IAirQualityDataDownloader<Models.Measurements>;

  using IAirlyInstallationDownloader
    = AirlyAnalyzer.Client.IAirQualityDataDownloader<Models.Installation>;

  public class ProgramController : IHostedService, IDisposable
  {
    private readonly IAirlyMeasurementsDownloader _airlyMeasurementsDownloader;
    private readonly IAirlyInstallationDownloader _airlyInstallationDownloader;

    private readonly IForecastErrorsCalculator _forecastErrorsCalculator;
    private readonly ILogger<ProgramController> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly List<short> _installationIds;
    private readonly short _idForAllInstallations;
    private readonly short _minNumberOfMeasurements;

    private UnitOfWork _unitOfWork;
    private Timer _timer;

    public ProgramController(
        UnitOfWork unitOfWork,
        IForecastErrorsCalculator forecastErrorsCalculator = null,
        List<short> installationIDsList = null,
        short idForAllInstallations = -1,
        IAirlyMeasurementsDownloader airlyMeasurementsDownloader = null,
        IAirlyInstallationDownloader airlyInstallationDownloader = null,
        short minNumberOfMeasurements = 24)
    {
      _unitOfWork = unitOfWork;
      _forecastErrorsCalculator = forecastErrorsCalculator;
      _airlyMeasurementsDownloader = airlyMeasurementsDownloader;
      _airlyInstallationDownloader = airlyInstallationDownloader;

      _installationIds = installationIDsList;

      _minNumberOfMeasurements = minNumberOfMeasurements;
      _idForAllInstallations = idForAllInstallations;
    }

    public ProgramController(
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<ProgramController> logger = null)
    {
      _serviceProvider = serviceProvider;
      _logger = logger;

      _minNumberOfMeasurements = config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _installationIds = config.GetSection(
          "AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _idForAllInstallations = config.GetValue<short>(
          "AppSettings:AirlyApi:IdForAllInstallations");

      _airlyMeasurementsDownloader = serviceProvider
          .GetRequiredService<IAirlyMeasurementsDownloader>();

      _airlyInstallationDownloader = serviceProvider
        .GetRequiredService<IAirlyInstallationDownloader>();

      _forecastErrorsCalculator = serviceProvider
          .GetRequiredService<IForecastErrorsCalculator>();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
      _logger?.LogInformation("ProgramController is starting");

      _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

      return Task.CompletedTask;
    }

    public async void DoWork(object state)
    {
      using (var scope = _serviceProvider.CreateScope())
      {
        _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        var installations = await DownloadInstallationInfos();

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

    public async Task<List<Installation>> DownloadInstallationInfos()
    {
      var installations = new List<Installation>();

      foreach (short installationId in _installationIds)
      {
        var dbInstallationInfo = await _unitOfWork
            .InstallationsRepository.GetById(installationId);

        var now = DateTime.UtcNow.Date;

        if (dbInstallationInfo == null
            || (now - dbInstallationInfo.RequestDate.Date).TotalDays >= 7)
        {
          var installation = await _airlyInstallationDownloader
              .DownloadAirQualityData(installationId);

          installations.Add(installation);
        }
      }

      return installations;
    }

    public List<InstallationInfo> ConvertInstallations(
        List<Installation> installations)
    {
      var installationInfos = new List<InstallationInfo>();

      foreach (var installation in installations)
      {
        installationInfos.Add(
            installation.ConvertToInstallationInfo(DateTime.UtcNow.Date));
      }

      return installationInfos;
    }

    public async Task<(List<AirQualityMeasurement>, List<AirQualityForecast>)>
        DownloadAllAirQualityData()
    {
      _logger?.LogInformation("DownloadAllAirQualityData() is starting");

      var newMeasurements = new List<AirQualityMeasurement>();
      var newForecasts = new List<AirQualityForecast>();

      var requestDateTime = DateTime.UtcNow;

      foreach (short installationId in _installationIds)
      {
        var lastMeasurementDate = await _unitOfWork
            .MeasurementRepository.GetLastDate(installationId);

        if ((requestDateTime - lastMeasurementDate).TotalHours
            >= _minNumberOfMeasurements)
        {
          var responseMeasurements = await _airlyMeasurementsDownloader
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

      await _unitOfWork.MeasurementRepository.AddListAsync(newMeasurements);
      await _unitOfWork.ForecastRepository.AddListAsync(newForecasts);
      await _unitOfWork.SaveChangesAsync();

      return Math.Min(newMeasurements.Count, newForecasts.Count);
    }

    public async Task<(List<AirQualityForecastError>, List<AirQualityForecastError>)>
        CalculateForecastErrors()
    {
      _logger?.LogInformation("CalculateForecastErrors() is starting");

      var allHourlyForecastErrors = new List<AirQualityForecastError>();
      var allDailyForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIds)
      {
        var (newMeasurements, newForecasts) = await _unitOfWork
            .ForecastErrorRepository.SelectDataToProcessing(installationId);

        var hourlyForecastErrors = _forecastErrorsCalculator
            .CalculateHourly(installationId, newMeasurements, newForecasts);

        allHourlyForecastErrors.AddRange(hourlyForecastErrors);

        allDailyForecastErrors.AddRange(_forecastErrorsCalculator.CalculateDaily(
            installationId, _minNumberOfMeasurements, hourlyForecastErrors));
      }

      return (allHourlyForecastErrors, allDailyForecastErrors);
    }

    public async Task<int> SaveForecastErrors(
        List<AirQualityForecastError> hourlyForecastErrors,
        List<AirQualityForecastError> dailyForecastErrors)
    {
      _logger?.LogInformation("SaveForecastErrors() is starting");

      await _unitOfWork.ForecastErrorRepository
          .AddListAsync(hourlyForecastErrors);

      await _unitOfWork.ForecastErrorRepository
          .AddListAsync(dailyForecastErrors);

      await _unitOfWork.SaveChangesAsync();

      return dailyForecastErrors.Count;
    }

    public async Task<List<AirQualityForecastError>> CalculateTotalForecastErrors()
    {
      _logger?.LogInformation("CalculateTotalForecastErrors() is starting");

      var newTotalForecastErrors = new List<AirQualityForecastError>();

      // Calculating total forecast errors for each installation
      foreach (short installationId in _installationIds)
      {
        var dailyForecastErrors = await _unitOfWork.ForecastErrorRepository.Get(
            fe => fe.InstallationId == installationId
               && fe.ErrorType == ForecastErrorType.Daily);

        if (dailyForecastErrors.Count > 0)
        {
          var installationForecastError = _forecastErrorsCalculator
              .CalculateTotal(installationId, dailyForecastErrors);

          newTotalForecastErrors.Add(installationForecastError);
        }
      }

      if (newTotalForecastErrors.Count > 0)
      {
        // Calculating total forecast error from all installations
        var totalForecastError = _forecastErrorsCalculator
            .CalculateTotal(_idForAllInstallations, newTotalForecastErrors);

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

        await _unitOfWork.ForecastErrorRepository
            .AddListAsync(newTotalForecastErrors);

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