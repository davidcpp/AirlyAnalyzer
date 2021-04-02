namespace AirlyAnalyzer.UnitTests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using static AirlyAnalyzer.UnitTests.Helpers.ModelUtilities;
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

      _downloaderMock = new Mock<IAirQualityDataDownloader<Installation>>();
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
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()), Times.Never());
    }

    [Fact]
    public async Task download_for_all_installations_when_no_data_in_database()
    {
      // Arrange
      var downloadedData = new Installation();

      foreach (short installationId in _installationIds)
      {
        _downloaderMock.Setup(x => x.DownloadAirQualityData(installationId))
                       .ReturnsAsync(downloadedData);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIDsList: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()),
          Times.Exactly(_installationIds.Count));
    }

    [Fact]
    public async Task downloads_data_for_installations_not_present_in_database()
    {
      // Arrange
      var downloadedData = new Installation();

      var modifiedInstallationIds = new List<short>();

      short newInstallationId = 1;
      while (_installationIds.Contains(newInstallationId))
      {
        newInstallationId++;
      }

      modifiedInstallationIds.Add(newInstallationId);
      modifiedInstallationIds.AddRange(
          _installationIds.GetRange(0, _installationIds.Count - 1));

      foreach (short installationId in _installationIds)
      {
        var exampleInstallationInfo = new InstallationInfo
        {
          InstallationId = installationId,
          Address = new Address()
          {
            City = "Pniewy",
            Country = "Poland",
            Street = "Poznańska",
            Number = "15",
          }
        };
        _context.InstallationInfos.Add(exampleInstallationInfo);
        _context.SaveChanges();
      }

      _downloaderMock.Setup(x => x.DownloadAirQualityData(newInstallationId))
                     .ReturnsAsync(downloadedData);

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIDsList: modifiedInstallationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(newInstallationId), Times.Once);

      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()), Times.Once);

      Assert.Single(installations);
    }

    [Fact]
    public async Task downloads_when_at_least_week_elapsed_from_last_update()
    {
      // Arrange
      short outOfDateInstallationId = _installationIds[0];
      var outOfDateRequestDate = DateTime.UtcNow.Date.AddDays(-7);

      var exampleInstallationInfo = GetTestInstallationInfo(
          outOfDateInstallationId, outOfDateRequestDate);

      _context.InstallationInfos.Add(exampleInstallationInfo);

      for (int i = 1; i < _installationIds.Count; i++)
      {
        exampleInstallationInfo = GetTestInstallationInfo(
             _installationIds[i], outOfDateRequestDate.AddDays(1));

        _context.InstallationInfos.Add(exampleInstallationInfo);
      }
      _context.SaveChanges();

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIDsList: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(outOfDateInstallationId), Times.Once);

      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()), Times.Once);

      Assert.Single(installations);
    }

    [Fact]
    public async Task returns_data_for_all_installations()
    {
      // Arrange
      var downloadedData = new Installation();

      foreach (short installationId in _installationIds)
      {
        _downloaderMock.Setup(x => x.DownloadAirQualityData(installationId))
                       .ReturnsAsync(downloadedData);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIDsList: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      Assert.Equal(_installationIds.Count, installations.Count);
    }
  }
}
