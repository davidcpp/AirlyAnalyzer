namespace AirlyAnalyzer.Services
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;

  public class AirQualityForecastController : IHostedService, IDisposable
  {
    private readonly IOpenWeatherApiDownloader _openWeatherApiDownloader;

    private readonly ILogger<AirQualityForecastController> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly List<short> _installationIds;
    private readonly short _forecastUpdateHoursPeriod;

    private UnitOfWork _unitOfWork;
    private Timer _timer;

    public AirQualityForecastController(
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<AirQualityForecastController> logger = null,
        UnitOfWork unitOfWork = null /* Unit Tests variant */)
    {
      _serviceProvider = serviceProvider;
      _logger = logger;

      if (unitOfWork != null)
      {
        _unitOfWork = unitOfWork;
      }

      _forecastUpdateHoursPeriod = config.GetValue<short>(
          "AppSettings:AirQualityForecast:UpdateHoursPeriod");

      _installationIds = config.GetSection(
          "AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _openWeatherApiDownloader
          = serviceProvider.GetRequiredService<IOpenWeatherApiDownloader>();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
      _logger?.LogInformation("AirQualityForecastController is starting");

      return Task.CompletedTask;
    }

    public async Task<List<OpenWeatherForecast>> DownloadHourlyWeatherForecasts()
    {
      var hourlyWeatherForecasts = new List<OpenWeatherForecast>();

      var requestDateTime = DateTime.UtcNow;

      foreach (short installationId in _installationIds)
      {
        var lastForecastDate
            = await _unitOfWork.ForecastRepository.GetLastDate(
                installationId, AirQualityDataSource.App);

        var installationInfo = await
            _unitOfWork.InstallationsRepository.GetById(installationId);

        if ((requestDateTime - lastForecastDate).TotalHours
            >= _forecastUpdateHoursPeriod || installationInfo == null )
        {
          var weatherForecast = await _openWeatherApiDownloader
              .DownloadHourlyWeatherForecast(
                  installationInfo.Location.Latitude,
                  installationInfo.Location.Longitude);

          hourlyWeatherForecasts.Add(weatherForecast);
        }
        else
        {
          hourlyWeatherForecasts.Add(new OpenWeatherForecast());
        }
      }

      return hourlyWeatherForecasts;
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
