namespace AirlyAnalyzer.UnitTests.OpenWeatherApiDownloaderTests
{
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Microsoft.Extensions.Configuration;
  using Moq;
  using Xunit;

  public class DownloadHourlyWeatherForecastTest
  {
    private const float _latitude = 50.0f;
    private const float _longtitude = 20.0f;

    private readonly IConfiguration _config;

    public DownloadHourlyWeatherForecastTest()
    {
      _config = ConfigUtilities.GetApplicationConfig();
    }

    [Fact]
    public async Task calls_download_string_method_with_correct_address_parameter()
    {
      // Arrange
      const string response = "{}";
      var webClientMock = new Mock<IWebClientAdapter>();

      const string exclude = "minutely,daily,current,alerts";

      string uri = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:Uri");

      string apiKeyParameter = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:AppIdParameter");

      string unitsParameter = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:UnitsParameter");

      string apiKey = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:Key");

      string units = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:Units");

      string forecastUri = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:OneCallUri");

      string latitudeParameter = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:LatitudeParameter");

      string longtitudeParameter = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:LongtitudeParameter");

      string excludeParameter = _config.GetValue<string>(
          "AppSettings:OpenWeatherApi:ExcludeParameter");

      string address = forecastUri
          + apiKeyParameter + apiKey + "&"
          + latitudeParameter + _latitude + "&"
          + longtitudeParameter + _longtitude + "&"
          + unitsParameter + units + "&"
          + excludeParameter + exclude;

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(address))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var openWeatherApiDownloader
          = new OpenWeatherApiDownloader(_config, webClientMock.Object);

      // Act
      var openWeatherForecast = await openWeatherApiDownloader
          .DownloadHourlyWeatherForecast(_latitude, _longtitude);

      // Assert
      webClientMock.Verify(_ => _.DownloadStringTaskAsync(address), Times.Once());
    }

    [Fact]
    public async Task calls_download_string_method_once()
    {
      // Arrange
      const string response = "{}";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var openWeatherApiDownloader
          = new OpenWeatherApiDownloader(_config, webClientMock.Object);

      // Act
      var openWeatherForecast = await openWeatherApiDownloader
          .DownloadHourlyWeatherForecast(_latitude, _longtitude);

      // Assert
      webClientMock.Verify(
          _ => _.DownloadStringTaskAsync(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task returns_not_null_forecast_object_when_api_response_is_correct_json_object()
    {
      // Arrange
      string response = OpenWeatherApiUtilities.GetTestOpenWeatherApiForecastJson();
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var openWeatherApiDownloader
          = new OpenWeatherApiDownloader(_config, webClientMock.Object);

      // Act
      var openWeatherForecast = await openWeatherApiDownloader
          .DownloadHourlyWeatherForecast(_latitude, _longtitude);

      // Assert
      Assert.NotEqual(0, openWeatherForecast.TimeZoneOffset);
      Assert.NotNull(openWeatherForecast.HourlyForecast);
      Assert.NotEmpty(openWeatherForecast.HourlyForecast);
    }

    [Fact]
    public async Task returns_empty_forecast_object_when_api_response_is_empty()
    {
      // Arrange
      const string response = "";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var openWeatherApiDownloader
          = new OpenWeatherApiDownloader(_config, webClientMock.Object);

      // Act
      var openWeatherForecast = await openWeatherApiDownloader
          .DownloadHourlyWeatherForecast(_latitude, _longtitude);

      // Assert
      Assert.NotNull(openWeatherForecast);
    }

    [Fact]
    public async Task returns_empty_forecast_object_when_api_response_is_empty_json_object()
    {
      // Arrange
      const string response = "{}";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var openWeatherApiDownloader
          = new OpenWeatherApiDownloader(_config, webClientMock.Object);

      // Act
      var openWeatherForecast = await openWeatherApiDownloader
          .DownloadHourlyWeatherForecast(_latitude, _longtitude);

      // Assert
      Assert.NotNull(openWeatherForecast);
    }

    [Fact]
    public async Task returns_empty_forecast_object_when_api_response_is_mismatched_json_object()
    {
      // Arrange
      const string response = @"{ ""field1"": {}, ""field2"": 0 }";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var openWeatherApiDownloader
          = new OpenWeatherApiDownloader(_config, webClientMock.Object);

      // Act
      var openWeatherForecast = await openWeatherApiDownloader
          .DownloadHourlyWeatherForecast(_latitude, _longtitude);

      // Assert
      Assert.NotNull(openWeatherForecast);
    }

    [Fact]
    public async Task returns_empty_forecast_object_when_api_response_is_null()
    {
      // Arrange
      const string response = null;
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var openWeatherApiDownloader
          = new OpenWeatherApiDownloader(_config, webClientMock.Object);

      // Act
      var openWeatherForecast = await openWeatherApiDownloader
          .DownloadHourlyWeatherForecast(_latitude, _longtitude);

      // Assert
      Assert.NotNull(openWeatherForecast);
    }
  }
}
