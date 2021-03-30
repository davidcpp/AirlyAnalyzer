namespace AirlyAnalyzer.UnitTests.AirQualityDataDownloaderTests
{
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Tests.Models;
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
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(
          _installationUri + _installationId.ToString()))
                   .ReturnsAsync("{}");

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
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync("{}");

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
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync("");

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
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync("{}");

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
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(@"{ ""field1"": {}, ""field2"": 0 }");

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
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync((string)null);

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
