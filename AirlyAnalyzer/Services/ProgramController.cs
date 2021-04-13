namespace AirlyAnalyzer.Services
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

  using IAirlyMeasurementsDownloader
      = AirlyAnalyzer.Client.IAirQualityDataDownloader<Models.Measurements>;

  using IAirlyInstallationDownloader
      = AirlyAnalyzer.Client.IAirQualityDataDownloader<Models.Installation>;

  public class ProgramController : IHostedService, IDisposable
  {
    private readonly IAirlyMeasurementsDownloader _airlyMeasurementsDownloader;
    private readonly IAirlyInstallationDownloader _airlyInstallationDownloader;

    private readonly List<IForecastErrorsCalculator> _forecastErrorsCalculators;
    private readonly ILogger<ProgramController> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly List<short> _installationIds;

    private readonly short _idForAllInstallations;
    private readonly short _installationUpdateDaysPeriod;
    private readonly short _minNumberOfMeasurements;

    private IForecastErrorsCalculator _forecastErrorsCalculator;
    private ForecastErrorsRepository _forecastErrorsRepository;
    private UnitOfWork _unitOfWork;
    private Timer _timer;

    public ProgramController(
        UnitOfWork unitOfWork,
        IForecastErrorsCalculator forecastErrorsCalculator = null,
        List<short> installationIds = null,
        short idForAllInstallations = -1,
        IAirlyMeasurementsDownloader airlyMeasurementsDownloader = null,
        IAirlyInstallationDownloader airlyInstallationDownloader = null,
        short minNumberOfMeasurements = 24,
        short installationUpdateDaysPeriod = 7)
    {
      _unitOfWork = unitOfWork;
      _forecastErrorsCalculator = forecastErrorsCalculator;
      _airlyMeasurementsDownloader = airlyMeasurementsDownloader;
      _airlyInstallationDownloader = airlyInstallationDownloader;

      _installationIds = installationIds;

      _minNumberOfMeasurements = minNumberOfMeasurements;
      _idForAllInstallations = idForAllInstallations;
      _installationUpdateDaysPeriod = installationUpdateDaysPeriod;
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

      _installationUpdateDaysPeriod = config.GetValue<short>(
          "AppSettings:AirlyApi:InstallationUpdateDaysPeriod");

      _installationIds = config.GetSection(
          "AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _idForAllInstallations = config.GetValue<short>(
          "AppSettings:AirlyApi:IdForAllInstallations");

      _airlyMeasurementsDownloader = serviceProvider
          .GetRequiredService<IAirlyMeasurementsDownloader>();

      _airlyInstallationDownloader = serviceProvider
        .GetRequiredService<IAirlyInstallationDownloader>();

      _forecastErrorsCalculators = serviceProvider
          .GetServices<IForecastErrorsCalculator>().ToList();
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

        if (installations.Count > 0)
        {
          var installationInfos = ConvertInstallations(installations);
          await UpdateInstallationInfos(installationInfos);
        }

        var newMeasurementsList = await DownloadAllAirQualityData();

        if (newMeasurementsList.Count > 0)
        {
          var (newMeasurements, newForecasts)
              = await ConvertAllAirQualityData(newMeasurementsList);
          await SaveAllAirQualityData(newMeasurements, newForecasts);

          for (int i = 0; i < _forecastErrorsCalculators.Count; i++)
          {
            _forecastErrorsCalculator = _forecastErrorsCalculators[i];

            switch (_forecastErrorsCalculator)
            {
              case ForecastErrorsCalculator _:
                _forecastErrorsRepository = _unitOfWork.PlainForecastErrorRepository;
                break;
              case ForecastScaleErrorsCalculator _:
                _forecastErrorsRepository = _unitOfWork.ScaleForecastErrorRepository;
                break;
            }

            var (hourlyErrors, dailyErrors) = await CalculateForecastErrors();
            await SaveForecastErrors(hourlyErrors, dailyErrors);

            var newTotalForecastErrors = await CalculateTotalForecastErrors();
            await UpdateTotalForecastErrors(newTotalForecastErrors);
          }
        }
      }
    }

    public async Task<List<Installation>> DownloadInstallationInfos()
    {
      _logger?.LogInformation("DownloadInstallationInfos() is starting");

      var installations = new List<Installation>();

      foreach (short installationId in _installationIds)
      {
        var dbInstallationInfo = await _unitOfWork
            .InstallationsRepository.GetById(installationId);

        bool noInstallationInDatabase = dbInstallationInfo == null;

        var now = DateTime.UtcNow.Date;
        var installationUpdateDate
            = dbInstallationInfo?.UpdateDate.Date ?? new DateTime();

        double fromLastUpdatePeriod = (now - installationUpdateDate).TotalDays;

        bool dataIsOutOfDate = fromLastUpdatePeriod >= _installationUpdateDaysPeriod;

        if (noInstallationInDatabase || dataIsOutOfDate)
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
      _logger?.LogInformation("ConvertInstallations() is starting");

      var installationInfos = new List<InstallationInfo>();

      foreach (var installation in installations)
      {
        installationInfos.Add(
            installation.ConvertToInstallationInfo(DateTime.UtcNow.Date));
      }

      return installationInfos;
    }

    public async Task UpdateInstallationInfos(
        List<InstallationInfo> newInstallationInfos)
    {
      _logger?.LogInformation("UpdateInstallationInfos() is starting");

      foreach (var installationInfo in newInstallationInfos)
      {
        if (installationInfo.InstallationId == 0)
        {
          continue;
        }

        if (_unitOfWork.InstallationsRepository.Contains(installationInfo))
        {
          await _unitOfWork.InstallationsRepository
              .Delete(installationInfo.InstallationId);

          await _unitOfWork.SaveChangesAsync();
        }

        await _unitOfWork.InstallationsRepository.AddAsync(installationInfo);
        await _unitOfWork.SaveChangesAsync();
      }

      var dbInstallationInfos = await _unitOfWork.InstallationsRepository.Get();

      // delete informations for out-of-date installations
      foreach (var dbInstallationInfo in dbInstallationInfos)
      {
        if (!_installationIds.Contains(dbInstallationInfo.InstallationId))
        {
          await _unitOfWork.InstallationsRepository
              .Delete(dbInstallationInfo.InstallationId);
        }
      }

      await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<Measurements>> DownloadAllAirQualityData()
    {
      _logger?.LogInformation("DownloadAllAirQualityData() is starting");

      var allMeasurements = new List<Measurements>();

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

          allMeasurements.Add(responseMeasurements);
        }
        else
        {
          allMeasurements.Add(new Measurements());
        }
      }

      return allMeasurements;
    }

    public async Task<(List<AirQualityMeasurement>, List<AirQualityForecast>)>
        ConvertAllAirQualityData(List<Measurements> measurementsList)
    {
      _logger?.LogInformation("ConvertAllAirQualityData() is starting");

      var requestDateTime = DateTime.UtcNow;

      var newMeasurements = new List<AirQualityMeasurement>();
      var newForecasts = new List<AirQualityForecast>();

      for (int i = 0; i < measurementsList.Count; i++)
      {
        var installationAddress = (await _unitOfWork.InstallationsRepository
            .GetById(_installationIds[i]))?.Address ?? new Address();

        newMeasurements.AddRange(
            measurementsList[i].History.ConvertToAirQualityMeasurements(
                _installationIds[i], installationAddress, requestDateTime));

        newForecasts.AddRange(
            measurementsList[i].Forecast.ConvertToAirQualityForecasts(
                _installationIds[i], installationAddress, requestDateTime));
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
               && fe.Period == ForecastErrorPeriod.Day);

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
            fe => fe.Period == ForecastErrorPeriod.Total);

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