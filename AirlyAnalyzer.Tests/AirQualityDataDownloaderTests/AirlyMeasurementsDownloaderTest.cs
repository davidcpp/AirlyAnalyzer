namespace AirlyAnalyzer.Tests.AirQualityDataDownloaderTests
{
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Tests.Models;
  using Microsoft.Extensions.Configuration;
  using Moq;
  using Xunit;

  public class AirlyMeasurementsDownloaderTest
  {
    private const short _installationId = 1;
    private readonly string _measurementsUri;

    private readonly IConfiguration _config;

    public AirlyMeasurementsDownloaderTest()
    {
      _config = ConfigUtilities.GetApplicationConfig();

      _measurementsUri = _config.GetValue<string>(
          "AppSettings:AirlyApi:MeasurementsUri");
    }

    [Fact]
    public async Task calls_download_string_method_with_correct_address_parameter()
    {
      // Arrange
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(
          _measurementsUri + _installationId.ToString()))
                   .ReturnsAsync("{}");

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyMeasurementsDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      webClientMock.Verify(_ => _.DownloadStringTaskAsync(
          _measurementsUri + _installationId.ToString()), Times.Once());
    }

    [Fact]
    public async Task calls_download_string_method_once()
    {
      // Arrange
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync("{}");

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyMeasurementsDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      webClientMock.Verify(
          _ => _.DownloadStringTaskAsync(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task returns_empty_measurements_object_when_api_response_is_empty()
    {
      // Arrange
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync("");

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyMeasurementsDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      Assert.NotNull(measurements);
    }

    [Fact]
    public async Task returns_empty_measurements_object_when_api_response_is_empty_json_object()
    {
      // Arrange
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync("{}");

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyMeasurementsDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      Assert.NotNull(measurements);
    }

    [Fact]
    public async Task returns_empty_measurements_object_when_api_response_is_mismatched_json_object()
    {
      // Arrange
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync(@"{ ""field1"": {}, ""field2"": 0 }");

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyMeasurementsDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      Assert.NotNull(measurements);
    }

    [Fact]
    public async Task returns_empty_measurements_object_when_api_response_is_null()
    {
      // Arrange
      var webClientMock = new Mock<IWebClientAdapter>();

      webClientMock.Setup(_ => _.DownloadStringTaskAsync(It.IsAny<string>()))
                   .ReturnsAsync((string)null);

      webClientMock.SetupProperty(_ => _.Headers, new WebHeaderCollection());

      var airlyMeasurementsDownloader
          = new AirlyMeasurementsDownloader(_config, webClientMock.Object);

      // Act
      var measurements = await airlyMeasurementsDownloader
          .DownloadAirQualityData(_installationId);

      // Assert
      Assert.NotNull(measurements);
    }
  }
}
