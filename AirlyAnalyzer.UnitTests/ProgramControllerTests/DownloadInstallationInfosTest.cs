﻿namespace AirlyAnalyzer.UnitTests.ProgramControllerTests
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
          installationIds: emptyInstallationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()), Times.Never());

      Assert.Empty(installations);
    }

    [Fact]
    public async Task does_not_download_when_data_is_up_to_date_and_all_installation_ids_in_database()
    {
      // Arrange
      const short installationUpdateDaysPeriod = 3;
      var upToDateUpdateDate = DateTime.UtcNow.Date.AddDays(-1);

      for (int i = 0; i < _installationIds.Count; i++)
      {
        var exampleInstallationInfo = GetTestInstallationInfo(
              _installationIds[i], upToDateUpdateDate);

        _context.InstallationInfos.Add(exampleInstallationInfo);
      }

      _context.SaveChanges();

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object,
          installationUpdateDaysPeriod: installationUpdateDaysPeriod);

      // Act
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()), Times.Never);

      Assert.Empty(installations);
    }

    [Fact]
    public async Task downloads_for_all_installations_when_no_data_in_database()
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
          installationIds: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadAirQualityData(It.IsAny<short>()),
          Times.Exactly(_installationIds.Count));

      Assert.Equal(_installationIds.Count, installations.Count);
    }

    [Fact]
    public async Task downloads_for_installations_not_present_in_database()
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
        var exampleInstallationInfo
            = GetTestInstallationInfo(installationId, DateTime.UtcNow.Date);

        _context.InstallationInfos.Add(exampleInstallationInfo);
      }

      _context.SaveChanges();

      _downloaderMock.Setup(x => x.DownloadAirQualityData(newInstallationId))
                     .ReturnsAsync(downloadedData);

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: modifiedInstallationIds,
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
    public async Task downloads_when_at_least_update_period_elapsed_from_last_update()
    {
      // Arrange
      short outOfDateInstallationId = _installationIds[0];
      const short installationUpdateDaysPeriod = 3;
      var outOfDateUpdateDate
          = DateTime.UtcNow.Date.AddDays(-installationUpdateDaysPeriod);

      var exampleInstallationInfo = GetTestInstallationInfo(
          outOfDateInstallationId, outOfDateUpdateDate);

      _context.InstallationInfos.Add(exampleInstallationInfo);

      for (int i = 1; i < _installationIds.Count; i++)
      {
        exampleInstallationInfo = GetTestInstallationInfo(
             _installationIds[i], outOfDateUpdateDate.AddDays(2));

        _context.InstallationInfos.Add(exampleInstallationInfo);
      }

      _context.SaveChanges();

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object,
          installationUpdateDaysPeriod: installationUpdateDaysPeriod);

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
          installationIds: _installationIds,
          airlyInstallationDownloader: _downloaderMock.Object);

      // Act
      var installations = await programController.DownloadInstallationInfos();

      // Assert
      Assert.Equal(_installationIds.Count, installations.Count);
    }
  }
}
