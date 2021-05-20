namespace AirlyAnalyzer.UnitTests.AirQualityForecastControllerTests
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

    public DownloadHourlyWeatherForecastsTest(RepositoryFixture fixture)
    {
      _unitOfWork = fixture.UnitOfWork;
      _context = fixture.Context;

      _downloaderMock = new Mock<IOpenWeatherApiDownloader>();

      _context.Clear();
    }

    [Fact]
    public async Task does_not_download_when_no_installations()
    {
      // Arrange
      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", ""),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

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

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", ""),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Empty(weatherForecastsList);
    }

    [Fact]
    public async Task does_not_download_when_air_quality_forecasts_are_up_to_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(7 * updateHoursPeriod) + 1);
      var installationIds = new List<short> { 2, 4, 6 };

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Never());
    }

    [Fact]
    public async Task returns_new_empty_forecasts_for_all_installations_when_air_quality_forecasts_are_up_to_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(7 * updateHoursPeriod) + 1);
      var installationIds = new List<short> { 2, 4, 6 };

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(installationIds.Count, weatherForecastsList.Count);
      Assert.NotNull(weatherForecastsList[0]);
      Assert.NotNull(weatherForecastsList[1]);
      Assert.NotNull(weatherForecastsList[2]);
    }

    [Fact]
    public async Task downloads_for_all_installations_when_all_air_quality_forecasts_are_out_of_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(7 * updateHoursPeriod));
      var installationIds = new List<short> { 2, 4, 6 };

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(installationIds.Count));
    }

    [Fact]
    public async Task returns_downloaded_for_all_installations_when_all_air_quality_forecasts_are_out_of_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(7 * updateHoursPeriod));
      var installationIds = new List<short> { 2, 4, 6 };

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
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(installationIds.Count, weatherForecastsList.Count);
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
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(7 * updateHoursPeriod));
      var installationIds = new List<short> { 2, 4, 6 };

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      _context.AddForecastsToDatabase(
          installationIds[1],
          startDate.AddDays(numberOfDays),
          1,
          updateHoursPeriod,
          AirQualityDataSource.App);

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(installationIds.Count - 1));
    }

    [Fact]
    public async Task returns_downloaded_for_some_installations_when_some_air_quality_forecasts_are_out_of_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(7 * updateHoursPeriod));
      var installationIds = new List<short> { 2, 4, 6 };

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
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      _context.AddForecastsToDatabase(
          installationIds[1],
          startDate.AddDays(numberOfDays),
          1,
          updateHoursPeriod,
          AirQualityDataSource.App);

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(installationIds.Count, weatherForecastsList.Count);
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
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(7 * updateHoursPeriod));
      var installationIds = new List<short> { 2, 4, 6 };

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      _context.AddForecastsToDatabase(
          installationIds[1],
          startDate.AddDays(numberOfDays),
          1,
          1,
          AirQualityDataSource.Airly);

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(installationIds.Count));
    }

    [Fact]
    public async Task returns_downloaded_when_up_to_date_air_quality_forecasts_are_not_from_app_source()
    {
      // Arrange
      const short numberOfDays = 3;
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(7 * updateHoursPeriod));
      var installationIds = new List<short> { 2, 4, 6 };

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
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityDataSource.App);

      _context.AddForecastsToDatabase(
          installationIds[1],
          startDate.AddDays(numberOfDays),
          1,
          updateHoursPeriod,
          AirQualityDataSource.Airly);

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirQualityForecast:UpdateHoursPeriod",
            updateHoursPeriod.ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(installationIds.Count, weatherForecastsList.Count);
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
      var installationIds = new List<short> { 2, 4, 6 };

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(installationIds.Count));
    }

    [Fact]
    public async Task returns_downloaded_for_all_installations_when_no_air_quality_forecasts_in_database()
    {
      // Arrange
      var installationIds = new List<short> { 2, 4, 6 };

      var downloadedData = new OpenWeatherForecast();
      downloadedData.HourlyForecast.Add(new OpenWeatherForecastObject());

      _downloaderMock.Setup(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()))
                     .ReturnsAsync(downloadedData);

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      var configInstallationIds = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:0", installationIds[0].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:1", installationIds[1].ToString()),
        new KeyValuePair<string, string>(
            "AppSettings:AirlyApi:InstallationIds:2", installationIds[2].ToString()),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config, unitOfWork: _unitOfWork);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      Assert.Equal(installationIds.Count, weatherForecastsList.Count);
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
  }
}
