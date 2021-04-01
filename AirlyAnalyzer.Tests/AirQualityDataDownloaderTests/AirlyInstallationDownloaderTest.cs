namespace AirlyAnalyzer.Tests.AirQualityDataDownloaderTests
{
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Tests.Helpers;
  using Microsoft.Extensions.Configuration;
  using Moq;
  using Xunit;

  public class AirlyInstallationDownloaderTest
  {
    private const short _installationId = 1;
    private readonly string _installationUri;

    private readonly IConfiguration _config;

    public AirlyInstallationDownloaderTest()
    {
      _config = ConfigUtilities.GetApplicationConfig();

      _installationUri = _config.GetValue<string>(
          "AppSettings:AirlyApi:InstallationUri");
    }

    [Fact]
    public async Task calls_download_string_method_with_correct_address_parameter()
    {
      // Arrange
      const string response = "{}";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(
          _installationUri + _installationId.ToString()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyInstallationDownloader
          = new AirlyInstallationDownloader(_config, webClientMock.Object);

      // Act
      var installation = await airlyInstallationDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      webClientMock.Verify(_ => _.DownloadStringTaskAsync(
          _installationUri + _installationId.ToString()), Times.Once());
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

      var airlyInstallationDownloader
          = new AirlyInstallationDownloader(_config, webClientMock.Object);

      // Act
      var installation = await airlyInstallationDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      webClientMock.Verify(
          _ => _.DownloadStringTaskAsync(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task returns_empty_installation_object_when_api_response_is_empty()
    {
      // Arrange
      const string response = "";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyInstallationDownloader
          = new AirlyInstallationDownloader(_config, webClientMock.Object);

      // Act
      var installation = await airlyInstallationDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      Assert.NotNull(installation);
    }

    [Fact]
    public async Task returns_empty_installation_object_when_api_response_is_empty_json_object()
    {
      // Arrange
      const string response = "{}";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyInstallationDownloader
          = new AirlyInstallationDownloader(_config, webClientMock.Object);

      // Act
      var installation = await airlyInstallationDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      Assert.NotNull(installation);
    }

    [Fact]
    public async Task returns_empty_installation_object_when_api_response_is_mismatched_json_object()
    {
      // Arrange
      const string response = @"{ ""field1"": {}, ""field2"": 0 }";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyInstallationDownloader
          = new AirlyInstallationDownloader(_config, webClientMock.Object);

      // Act
      var installation = await airlyInstallationDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      Assert.NotNull(installation);
    }

    [Fact]
    public async Task returns_empty_installation_object_when_api_response_is_null()
    {
      // Arrange
      const string response = null;
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyInstallationDownloader(_config, webClientMock.Object);

      // Act
      var installation = await airlyMeasurementsDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      Assert.NotNull(installation);
    }
  }
}
