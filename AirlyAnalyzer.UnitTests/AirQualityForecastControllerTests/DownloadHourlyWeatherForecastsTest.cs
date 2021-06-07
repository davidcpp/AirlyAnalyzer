namespace AirlyAnalyzer.UnitTests.forecastServiceTests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using static AirlyAnalyzer.UnitTests.Helpers.ModelUtilities;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Moq;
  using Xunit;

  [Collection("RepositoryTests")]
  public class DownloadHourlyWeatherForecastsTest
  {
    private readonly Mock<IOpenWeatherApiDownloader> _downloaderMock;

    private readonly UnitOfWork _unitOfWork;
    private readonly AirlyContext _context;
    private readonly List<InstallationInfo> _installationInfos;
    private readonly List<short> _installationIds;

    public DownloadHourlyWeatherForecastsTest(RepositoryFixture fixture)
    {
      _installationIds = new List<short> { 2, 4, 6 };

      _unitOfWork = fixture.UnitOfWork;
      _context = fixture.Context;

      _downloaderMock = new Mock<IOpenWeatherApiDownloader>();

      _context.Clear();

      _installationInfos = GetTestInstallationInfoList(_installationIds);
      _context.InstallationInfos.AddRange(_installationInfos);
      _context.SaveChanges();
    }

    [Fact]
    public async Task does_not_download_when_no_installations()
    {
      // Arrange
      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", ""),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Never());
    }

    [Fact]
    public async Task returns_new_empty_forecast_list_when_no_installations()
    {
      // Arrange
      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", ""),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Empty(weatherForecastsList);
    }

    [Fact]
    public async Task does_not_download_when_all_air_quality_forecasts_are_up_to_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short numberOfForecastsInDay = 24;
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var startDate = DateTime.UtcNow.AddHours(
          -(7 * updateHoursPeriod) + 1 + weatherForecastHoursNumber);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          _installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Never());
    }

    [Fact]
    public async Task returns_new_empty_forecasts_for_all_installations_when_all_air_quality_forecasts_are_up_to_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short numberOfForecastsInDay = 24;
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var startDate = DateTime.UtcNow.AddHours(
          -(7 * updateHoursPeriod) + 1 + weatherForecastHoursNumber);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          _installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(_installationIds.Count, weatherForecastsList.Count);
      Assert.NotNull(weatherForecastsList[0]);
      Assert.NotNull(weatherForecastsList[1]);
      Assert.NotNull(weatherForecastsList[2]);
    }

    [Fact]
    public async Task does_not_download_when_no_installation_infos()
    {
      // Arrange
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.InstallationInfos.RemoveRange(_context.InstallationInfos);
      await _context.SaveChangesAsync();

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Never());
    }

    [Fact]
    public async Task returns_new_empty_forecasts_for_all_installations_when_no_installation_infos()
    {
      // Arrange
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.InstallationInfos.RemoveRange(_context.InstallationInfos);
      await _context.SaveChangesAsync();

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(_installationIds.Count, weatherForecastsList.Count);
      Assert.NotNull(weatherForecastsList[0]);
      Assert.NotNull(weatherForecastsList[1]);
      Assert.NotNull(weatherForecastsList[2]);
    }

    [Fact]
    public async Task downloads_for_all_installations_when_all_air_quality_forecasts_are_out_of_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short numberOfForecastsInDay = 24;
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var startDate = DateTime.UtcNow.AddHours(
          -(7 * updateHoursPeriod) + weatherForecastHoursNumber);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          _installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(_installationIds.Count));
    }

    [Fact]
    public async Task returns_downloaded_for_all_installations_when_all_air_quality_forecasts_are_out_of_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short numberOfForecastsInDay = 24;
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var startDate = DateTime.UtcNow.AddHours(
          -(7 * updateHoursPeriod) + weatherForecastHoursNumber);

      var downloadedData = new OpenWeatherForecast();
      downloadedData.HourlyForecast.Add(new OpenWeatherForecastObject());

      _downloaderMock.Setup(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()))
                     .ReturnsAsync(downloadedData);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          _installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(_installationIds.Count, weatherForecastsList.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[0].HourlyForecast.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[1].HourlyForecast.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[2].HourlyForecast.Count);
    }

    [Fact]
    public async Task downloads_for_some_installations_when_some_air_quality_forecasts_are_out_of_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short numberOfForecastsInDay = 24;
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var startDate = DateTime.UtcNow.AddHours(
          -(7 * updateHoursPeriod) + weatherForecastHoursNumber);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          _installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      _context.AddForecastsToDatabase(
          _installationIds[1],
          startDate.AddDays(numberOfDays),
          1,
          updateHoursPeriod,
          AirQualityDataSource.App);

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(_installationIds.Count - 1));
    }

    [Fact]
    public async Task returns_downloaded_for_some_installations_when_some_air_quality_forecasts_are_out_of_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short numberOfForecastsInDay = 24;
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var startDate = DateTime.UtcNow.AddHours(
          -(7 * updateHoursPeriod) + weatherForecastHoursNumber);

      var downloadedData = new OpenWeatherForecast();
      downloadedData.HourlyForecast.Add(new OpenWeatherForecastObject());

      _downloaderMock.Setup(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()))
                     .ReturnsAsync(downloadedData);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          _installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      _context.AddForecastsToDatabase(
          _installationIds[1],
          startDate.AddDays(numberOfDays),
          1,
          updateHoursPeriod,
          AirQualityDataSource.App);

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(_installationIds.Count, weatherForecastsList.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[0].HourlyForecast.Count);
      Assert.NotNull(weatherForecastsList[1]);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[2].HourlyForecast.Count);
    }

    [Fact]
    public async Task downloads_when_up_to_date_air_quality_forecasts_are_not_from_app_source()
    {
      // Arrange
      const short numberOfDays = 3;
      const short numberOfForecastsInDay = 24;
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var startDate = DateTime.UtcNow.AddHours(
          -(7 * updateHoursPeriod) + weatherForecastHoursNumber);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          _installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      _context.AddForecastsToDatabase(
          _installationIds[1],
          startDate.AddDays(numberOfDays),
          1,
          1,
          AirQualityDataSource.Airly);

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(_installationIds.Count));
    }

    [Fact]
    public async Task returns_downloaded_when_up_to_date_air_quality_forecasts_are_not_from_app_source()
    {
      // Arrange
      const short numberOfDays = 3;
      const short numberOfForecastsInDay = 24;
      const short updateHoursPeriod = 12;
      const short weatherForecastHoursNumber = 24;

      var startDate = DateTime.UtcNow.AddHours(
          -(7 * updateHoursPeriod) + weatherForecastHoursNumber);

      var downloadedData = new OpenWeatherForecast();
      downloadedData.HourlyForecast.Add(new OpenWeatherForecastObject());

      _downloaderMock.Setup(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()))
                     .ReturnsAsync(downloadedData);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          _installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      _context.AddForecastsToDatabase(
          _installationIds[1],
          startDate.AddDays(numberOfDays),
          1,
          1,
          AirQualityDataSource.Airly);

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:WeatherForecastHoursNumber",
            weatherForecastHoursNumber.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(_installationIds.Count, weatherForecastsList.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[0].HourlyForecast.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[1].HourlyForecast.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[2].HourlyForecast.Count);
    }

    [Fact]
    public async Task downloads_for_all_installations_when_no_air_quality_forecasts_in_database()
    {
      // Arrange
      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(_installationIds.Count));
    }

    [Fact]
    public async Task returns_downloaded_for_all_installations_when_no_air_quality_forecasts_in_database()
    {
      // Arrange
      var downloadedData = new OpenWeatherForecast();
      downloadedData.HourlyForecast.Add(new OpenWeatherForecastObject());

      _downloaderMock.Setup(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()))
                     .ReturnsAsync(downloadedData);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(_installationIds.Count, weatherForecastsList.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[0].HourlyForecast.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[1].HourlyForecast.Count);
      Assert.Equal(
          downloadedData.HourlyForecast.Count,
          weatherForecastsList[2].HourlyForecast.Count);
    }

    [Fact]
    public async Task downloads_with_correct_parameters()
    {
      // Arrange
      var downloadedData = new List<OpenWeatherForecast>
      {
        new OpenWeatherForecast { TimeZoneOffset = 3600 },
        new OpenWeatherForecast { TimeZoneOffset = 7200 },
        new OpenWeatherForecast { TimeZoneOffset = 10800 },
      };

      var downloaderMock = new Mock<IOpenWeatherApiDownloader>(MockBehavior.Strict);
      var mockSequence = new MockSequence();

      for (int i = 0; i < _installationIds.Count; i++)
      {
        downloaderMock.InSequence(mockSequence)
            .Setup(x => x.DownloadHourlyWeatherForecast(
                _installationInfos[i].Location.Latitude,
                _installationInfos[i].Location.Longitude))
                      .ReturnsAsync(downloadedData[i]);
      }

      var services = new ServiceCollection();
      services.AddSingleton(downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(_installationIds.Count));
    }

    [Fact]
    public async Task returns_correct_weather_forecast_objects()
    {
      // Arrange
      var downloadedData = new List<OpenWeatherForecast>
      {
        new OpenWeatherForecast { TimeZoneOffset = 3600 },
        new OpenWeatherForecast { TimeZoneOffset = 7200 },
        new OpenWeatherForecast { TimeZoneOffset = 10800 },
      };

      var downloaderMock = new Mock<IOpenWeatherApiDownloader>(MockBehavior.Strict);
      var mockSequence = new MockSequence();

      for (int i = 0; i < _installationIds.Count; i++)
      {
        downloaderMock.InSequence(mockSequence)
            .Setup(x => x.DownloadHourlyWeatherForecast(
                It.IsAny<float>(), It.IsAny<float>()))
                      .ReturnsAsync(downloadedData[i]);
      }

      var services = new ServiceCollection();
      services.AddSingleton(downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", _installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", _installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", _installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configData)
          .Build();

      var forecastService = new ForecastService(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await forecastService.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(_installationIds.Count, weatherForecastsList.Count);
      Assert.Equal(
          downloadedData[0].TimeZoneOffset, weatherForecastsList[0].TimeZoneOffset);
      Assert.Equal(
          downloadedData[1].TimeZoneOffset, weatherForecastsList[1].TimeZoneOffset);
      Assert.Equal(
          downloadedData[2].TimeZoneOffset, weatherForecastsList[2].TimeZoneOffset);
    }
  }
}
