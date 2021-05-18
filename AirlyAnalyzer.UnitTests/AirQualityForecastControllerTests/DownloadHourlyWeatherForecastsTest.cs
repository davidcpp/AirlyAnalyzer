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
