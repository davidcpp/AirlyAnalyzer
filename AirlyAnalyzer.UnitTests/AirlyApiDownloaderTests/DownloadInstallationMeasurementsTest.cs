namespace AirlyAnalyzer.UnitTests.AirlyApiDownloaderTests
{
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Microsoft.Extensions.Configuration;
  using Moq;
  using Xunit;

  public class DownloadInstallationMeasurementsTest
  {
    private const short _installationId = 1;
    private readonly string _measurementsUri;
    private readonly string _measurementsUriParameters;

    private readonly IConfiguration _config;

    public DownloadInstallationMeasurementsTest()
    {
      _config = ConfigUtilities.GetApplicationConfig();

      _measurementsUri = _config.GetValue<string>(
          "AppSettings:AirlyApi:MeasurementsUri");

      _measurementsUriParameters = _config.GetValue<string>(
          "AppSettings:AirlyApi:MeasurementsUriParameters");
    }

    [Fact]
    public async Task calls_download_string_method_with_correct_address_parameter()
    {
      // Arrange
      const string response = "{}";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(
          _measurementsUri + _installationId.ToString()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyApiDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadInstallationMeasurements(_installationId);

      // Assert
      webClientMock.Verify(
          _ => _.DownloadStringTaskAsync(
              _measurementsUri
              + _installationId.ToString()
              + _measurementsUriParameters),
          Times.Once());
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

      var airlyMeasurementsDownloader
          = new AirlyApiDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadInstallationMeasurements(_installationId);

      // Assert
      webClientMock.Verify(
          _ => _.DownloadStringTaskAsync(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task returns_empty_measurements_object_when_api_response_is_empty()
    {
      // Arrange
      const string response = "";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyApiDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadInstallationMeasurements(_installationId);

      // Assert
      Assert.NotNull(measurements);
    }

    [Fact]
    public async Task returns_empty_measurements_object_when_api_response_is_empty_json_object()
    {
      // Arrange
      const string response = "{}";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyApiDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadInstallationMeasurements(_installationId);

      // Assert
      Assert.NotNull(measurements);
    }

    [Fact]
    public async Task returns_empty_measurements_object_when_api_response_is_mismatched_json_object()
    {
      // Arrange
      const string response = @"{ ""field1"": {}, ""field2"": 0 }";
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyApiDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadInstallationMeasurements(_installationId);

      // Assert
      Assert.NotNull(measurements);
    }

    [Fact]
    public async Task returns_empty_measurements_object_when_api_response_is_null()
    {
      // Arrange
      const string response = null;
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(response);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyApiDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadInstallationMeasurements(_installationId);

      // Assert
      Assert.NotNull(measurements);
    }
  }
}
