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
    }

    [Fact]
    public async Task does_not_download_when_no_installations()
    {
      // Arrange
      var downloadedData = new OpenWeatherForecast();

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
            "AppSettings:AirlyApi:InstallationIds:0", ""),
      };

      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(configInstallationIds)
          .Build();

      var airQualityForecastController = new AirQualityForecastController(
          serviceProvider, config);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Never());

      Assert.Empty(weatherForecastsList);
    }

    [Fact]
    public async Task does_not_download_when_air_quality_forecasts_is_up_to_date()
    {
      // Arrange
      const short numberOfDays = 3;
      const short updateHoursPeriod = 12;
      const short numberOfForecastsInDay = 24;

      var startDate = DateTime.UtcNow.AddHours(-(4 * updateHoursPeriod) + 1);
      var installationIds = new List<short> { 2, 4, 6 };

      var services = new ServiceCollection();
      services.AddSingleton(_downloaderMock.Object);
      var serviceProvider = services.BuildServiceProvider();

      _context.AddAllForecastsToDatabase(
          installationIds,
          startDate,
          numberOfDays,
          numberOfForecastsInDay,
          AirQualityForecastSource.App);

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
          serviceProvider, config);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Never());

      Assert.Empty(weatherForecastsList);
    }

    [Fact]
    public async Task downloads_for_all_installations()
    {
      // Arrange
      var installationIds = new List<short> { 2, 4, 6 };
      var downloadedData = new OpenWeatherForecast();

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
          serviceProvider, config);

      // Act
      var weatherForecastsList
          = await airQualityForecastController.DownloadHourlyWeatherForecasts();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadHourlyWeatherForecast(
              It.IsAny<float>(), It.IsAny<float>()),
          Times.Exactly(configInstallationIds.Count));

      Assert.Equal(installationIds.Count, weatherForecastsList.Count);
    }
  }
}
