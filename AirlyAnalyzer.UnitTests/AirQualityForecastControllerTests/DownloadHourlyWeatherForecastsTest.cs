namespace AirlyAnalyzer.UnitTests.AirQualityForecastControllerTests
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Services;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Moq;
  using Xunit;

  public class DownloadHourlyWeatherForecastsTest
  {
    private readonly Mock<IOpenWeatherApiDownloader> _downloaderMock;

    public DownloadHourlyWeatherForecastsTest()
    {
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
        new KeyValuePair<string, string>("AppSettings:AirlyApi:InstallationIds:0", ""),
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
    }

    [Fact]
    public async Task downloads_for_all_installations()
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
        new KeyValuePair<string, string>("AppSettings:AirlyApi:InstallationIds:0", "2"),
        new KeyValuePair<string, string>("AppSettings:AirlyApi:InstallationIds:1", "4"),
        new KeyValuePair<string, string>("AppSettings:AirlyApi:InstallationIds:2", "6"),
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
    }
  }
}
