namespace AirlyAnalyzer.UnitTests.ProgramControllerTests
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Moq;
  using Xunit;

  [Collection("RepositoryTests")]
  public class DownloadInstallationInfosTest
  {
    private readonly Mock<IAirQualityDataDownloader<Installation>> _downloaderMock;

    private readonly UnitOfWork _unitOfWork;
    private readonly AirlyContext _context;
    private readonly List<short> _installationIds;

    public DownloadInstallationInfosTest(RepositoryFixture fixture)
    {
      _unitOfWork = fixture.UnitOfWork;
      _context = fixture.Context;
      _installationIds = fixture.InstallationIds;

      _context.Clear();

      _downloaderMock = new Mock<IAirQualityDataDownloader<Installation>>(
          MockBehavior.Strict);
    }

    [Fact]
    public async Task does_not_download_when_no_installations()
    {
      // Arrange
      var downloadedData = new Installation();
      var emptyInstallationIds = new List<short>();

      _downloaderMock.Setup(x => x.DownloadAirQualityData(It.IsAny<short>()))
          .ReturnsAsync(downloadedData);

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIDsList: emptyInstallationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installationInfos = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()), Times.Never());
    }

    [Fact]
    public async Task download_for_all_installations_when_no_data_in_database()
    {
      // Arrange
      var downloadedData = new Installation();
      var mockSequence = new MockSequence();

      foreach (short installationId in _installationIds)
      {
        _downloaderMock.InSequence(mockSequence)
            .Setup(x => x.DownloadAirQualityData(installationId))
            .ReturnsAsync(downloadedData);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIDsList: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installationInfos = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()),
          Times.Exactly(_installationIds.Count));
    }

    [Fact]
    public async Task download_for_all_installations_when_all_installations_info_in_database()
    {
      // Arrange
      var downloadedData = new Installation();
      var mockSequence = new MockSequence();

      var exampleInstallationInfo = new InstallationInfo
      {
        Address = new Address()
        {
          City = "Pniewy",
          Country = "Poland",
          Street = "Poznańska",
          Number = "15",
        }
      };

      foreach (short installationId in _installationIds)
      {
        exampleInstallationInfo.InstallationId = installationId;
        _context.InstallationInfos.Add(exampleInstallationInfo);
      }

      foreach (short installationId in _installationIds)
      {
        _downloaderMock.InSequence(mockSequence)
            .Setup(x => x.DownloadAirQualityData(installationId))
            .ReturnsAsync(downloadedData);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIDsList: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installationInfos = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()),
          Times.Exactly(_installationIds.Count));
    }
  }
}
