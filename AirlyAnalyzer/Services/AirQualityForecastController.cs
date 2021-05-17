namespace AirlyAnalyzer.Services
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using AirlyAnalyzer.Models;

  public class AirQualityForecastController : IHostedService, IDisposable
  {
    private readonly IOpenWeatherApiDownloader _openWeatherApiDownloader;
    private readonly ILogger<AirQualityForecastController> _logger;

    private readonly List<short> _installationIds;

    private Timer _timer;

    public AirQualityForecastController(
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<AirQualityForecastController> logger = null)
    {
      _logger = logger;

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
      var openWeatherForecast = await _openWeatherApiDownloader
          .DownloadHourlyWeatherForecast(0.0f, 0.0f);

      return new List<OpenWeatherForecast> { openWeatherForecast };
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
