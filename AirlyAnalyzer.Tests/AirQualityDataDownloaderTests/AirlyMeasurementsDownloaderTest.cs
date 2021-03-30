namespace AirlyAnalyzer.UnitTests.AirQualityDataDownloaderTests
{
  using System;
  using System.IO;
  using System.Net;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
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
      string configFilePath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

      _config = new ConfigurationBuilder()
          .AddJsonFile(configFilePath)
          .Build();

      _measurementsUri = _config.GetValue<string>(
          "AppSettings:AirlyApi:MeasurementsUri");
    }

    [Fact]
    public async Task call_download_string_method_with_correct_address_parameter()
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
    public async Task return_empty_installation_object_when_api_respone_is_empty()
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
    public async Task return_empty_installation_object_when_api_respone_is_empty_json_object()
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
  }
}
