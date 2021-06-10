namespace AirlyAnalyzer.Services
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Models.Weather;
  using AirlyAnalyzerML.Model;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;

  using AccuWeatherForecast
      = System.Collections.Generic.List<Models.AccuWeatherForecastObject>;

  public class ForecastService : IHostedService, IDisposable
  {
    private readonly IOpenWeatherApiDownloader _openWeatherApiDownloader;
    private readonly IAccuWeatherApiDownloader _accuWeatherApiDownloader;

    private readonly ILogger<ForecastService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly List<short> _installationIds;
    private readonly short _forecastUpdateHoursPeriod;
    private readonly short _weatherForecastHoursNumber;

    private UnitOfWork _unitOfWork;
    private Timer _timer;

    public ForecastService(
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<ForecastService> logger = null,
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

      _weatherForecastHoursNumber = config.GetValue<short>(
          "AppSettings:AirQualityForecast:WeatherForecastHoursNumber");

      _installationIds = config.GetSection(
          "AppSettings:AirlyApi:InstallationIds").Get<List<short>>();

      _openWeatherApiDownloader
          = serviceProvider.GetRequiredService<IOpenWeatherApiDownloader>();

      _accuWeatherApiDownloader
          = serviceProvider.GetRequiredService<IAccuWeatherApiDownloader>();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
      _logger?.LogInformation("ForecastService is starting");

      _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

      return Task.CompletedTask;
    }

    public async void DoWork(object state)
    {
      using (var scope = _serviceProvider.CreateScope())
      {
        _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        var openWeatherForecasts = await DownloadHourlyOpenWeatherForecasts();
        var convertedOpenWeatherForecasts
            = ConvertHourlyOpenWeatherForecasts(openWeatherForecasts);

        var airQualityPredictions = PredictAirQuality(
            convertedOpenWeatherForecasts, AirQualityDataSource.App_OpenWeather);

        var accuWeatherForecasts = await DownloadHourlyAccuWeatherForecasts();

        await AddInstallationAddressToForecasts(airQualityPredictions);

        await _unitOfWork.ForecastRepository.AddListAsync(airQualityPredictions);
        await _unitOfWork.SaveChangesAsync();
      }
    }

    public async Task<List<OpenWeatherForecast>> DownloadHourlyOpenWeatherForecasts()
    {
      _logger?.LogInformation("DownloadHourlyOpenWeatherForecasts() is starting");

      var hourlyWeatherForecasts = new List<OpenWeatherForecast>();

      var requestDateTime = DateTime.UtcNow;

      foreach (short installationId in _installationIds)
      {
        var lastForecastDate
            = await _unitOfWork.ForecastRepository.GetLastDate(
                installationId, AirQualityDataSource.App_OpenWeather);

        // theoretic last weather request dateTime - WeatherForecastHoursNumber
        // could be different for previously calculated air quality forecast
        var lastRequestDateTime
            = lastForecastDate.AddHours(-_weatherForecastHoursNumber);

        bool forecastIsOutOfDate = (requestDateTime - lastRequestDateTime)
            .TotalHours >= _forecastUpdateHoursPeriod;

        var installationInfo = await
            _unitOfWork.InstallationsRepository.GetById(installationId);

        bool isInstallationInDatabase = installationInfo != null;

        if (forecastIsOutOfDate && isInstallationInDatabase)
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

    public async Task<List<AccuWeatherForecast>> DownloadHourlyAccuWeatherForecasts()
    {
      _logger?.LogInformation("DownloadHourlyAccuWeatherForecasts() is starting");

      var hourlyWeatherForecasts = new List<AccuWeatherForecast>();

      var requestDateTime = DateTime.UtcNow;

      foreach (short installationId in _installationIds)
      {
        var lastForecastDate
            = await _unitOfWork.ForecastRepository.GetLastDate(
                installationId, AirQualityDataSource.App_AccuWeather);

        // theoretic last weather request dateTime - WeatherForecastHoursNumber
        // could be different for previously calculated air quality forecast
        var lastRequestDateTime
            = lastForecastDate.AddHours(-_weatherForecastHoursNumber);

        bool forecastIsOutOfDate = (requestDateTime - lastRequestDateTime)
            .TotalHours >= _forecastUpdateHoursPeriod;

        var installationInfo = await
            _unitOfWork.InstallationsRepository.GetById(installationId);

        bool isInstallationInDatabase = installationInfo != null;

        if (forecastIsOutOfDate && isInstallationInDatabase)
        {
          var weatherForecast = await _accuWeatherApiDownloader
              .DownloadHourlyWeatherForecast(
                  installationInfo.Location.Latitude,
                  installationInfo.Location.Longitude);

          hourlyWeatherForecasts.Add(weatherForecast);
        }
        else
        {
          hourlyWeatherForecasts.Add(new AccuWeatherForecast());
        }
      }

      return hourlyWeatherForecasts;
    }

    public List<WeatherMeasurement> ConvertHourlyOpenWeatherForecasts(
        List<OpenWeatherForecast> weatherForecasts)
    {
      _logger?.LogInformation("ConvertHourlyOpenWeatherForecasts() is starting");

      var convertedWeatherForecasts = new List<WeatherMeasurement>();

      for (int i = 0; i < _installationIds.Count; i++)
      {
        for (int j = 1; j < weatherForecasts[i].HourlyForecast.Count
            && j < _weatherForecastHoursNumber + 1; j++)
        {
          var weatherForecastItem = weatherForecasts[i].HourlyForecast[j];

          var convertedWeatherForecast =
              weatherForecastItem.ConvertToWeatherMeasurement(_installationIds[i]);

          convertedWeatherForecasts.Add(convertedWeatherForecast);
        }
      }

      return convertedWeatherForecasts;
    }

    public List<AirQualityForecast> PredictAirQuality(
        List<WeatherMeasurement> weatherForecasts, AirQualityDataSource source)
    {
      _logger?.LogInformation("PredictAirQuality() is starting");

      var airQualityForecasts = new List<AirQualityForecast>();

      foreach (var weatherForecastItem in weatherForecasts)
      {
        var mlWeatherData = new ModelInput
        {
          Year = weatherForecastItem.Year,
          Month = weatherForecastItem.Month,
          Day = weatherForecastItem.Day,
          Hour = weatherForecastItem.Hour,
          InstallationId = weatherForecastItem.InstallationId,
          Humidity = weatherForecastItem.Humidity,
          Temperature = weatherForecastItem.Temperature,
          Visibility = weatherForecastItem.Visibility,
          WindSpeed = weatherForecastItem.WindSpeed,
        };

        var predictionResult = ConsumeModel.Predict(mlWeatherData);

        var forecastDateTime = new DateTime(
            mlWeatherData.Year,
            mlWeatherData.Month,
            mlWeatherData.Day,
            mlWeatherData.Hour,
            0,
            0,
            DateTimeKind.Utc);

        var airQualityForecast = new AirQualityForecast
        {
          InstallationId = (short)weatherForecastItem.InstallationId,
          FromDateTime = forecastDateTime.AddHours(-1),
          TillDateTime = forecastDateTime,
          RequestDateTime = DateTime.UtcNow,
          Source = source,
          AirlyCaqi = Convert.ToInt16(Math.Ceiling(predictionResult.Score))
        };

        airQualityForecasts.Add(airQualityForecast);
      }

      return airQualityForecasts;
    }

    private async Task AddInstallationAddressToForecasts(
        List<AirQualityForecast> airQualityForecasts)
    {
      var installationAddresses = new Dictionary<int, Address>();

      for (int i = 0; i < _installationIds.Count; i++)
      {
        var installationAddress = (await _unitOfWork.InstallationsRepository
            .GetById(_installationIds[i]))?.Address ?? new Address();

        installationAddresses[_installationIds[i]] = installationAddress;
      }

      foreach (var airQualityForecast in airQualityForecasts)
      {
        airQualityForecast.InstallationAddress
            = installationAddresses[airQualityForecast.InstallationId].ToString();
      }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
      _logger?.LogInformation("ForecastService is stopping");

      _timer?.Change(Timeout.Infinite, 0);

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }
  }
}
