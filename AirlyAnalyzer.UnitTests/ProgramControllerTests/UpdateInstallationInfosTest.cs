namespace AirlyAnalyzer.UnitTests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using static AirlyAnalyzer.UnitTests.Helpers.ModelUtilities;
  using Moq;
  using Xunit;

  [Collection("RepositoryTests")]
  public class UpdateInstallationInfosTest
  {
    private readonly Mock<IAirlyApiDownloader> _downloaderMock;

    private readonly UnitOfWork _unitOfWork;
    private readonly AirlyContext _context;
    private readonly List<short> _installationIds;

    public UpdateInstallationInfosTest(RepositoryFixture fixture)
    {
      _unitOfWork = fixture.UnitOfWork;
      _context = fixture.Context;
      _installationIds = fixture.InstallationIds;

      _context.Clear();

      _downloaderMock = new Mock<IAirlyApiDownloader>(MockBehavior.Strict);
    }

    [Fact]
    public async Task does_not_update_when_new_installation_info_list_with_empty_objects()
    {
      // Arrange
      var newInstallationInfos = new List<InstallationInfo>();

      foreach (short installationId in _installationIds)
      {
        var dbInstallationInfo = GetTestInstallationInfo(
            installationId, new DateTime());

        _context.InstallationInfos.Add(dbInstallationInfo);
      }

      _context.SaveChanges();

      for (int i = 0; i < _installationIds.Count + 1; i++)
      {
        var emptyInstallationInfo = new InstallationInfo();

        newInstallationInfos.Add(emptyInstallationInfo);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyApiDownloader: _downloaderMock.Object);

      // Act
      await programController.UpdateInstallationInfos(newInstallationInfos);

      var dbInstallationInfos
          = _context.InstallationInfos.OrderBy(i => i.InstallationId).ToList();

      // Assert
      Assert.Equal(_installationIds.Count, dbInstallationInfos.Count);

      for (int i = 0; i < dbInstallationInfos.Count; i++)
      {
        Assert.Equal(_installationIds[i], dbInstallationInfos[i].InstallationId);
      }
    }

    [Fact]
    public async Task does_not_update_when_new_installation_info_list_is_empty()
    {
      // Arrange
      var newInstallationInfos = new List<InstallationInfo>();

      foreach (short installationId in _installationIds)
      {
        var dbInstallationInfo = GetTestInstallationInfo(
            installationId, new DateTime());

        _context.InstallationInfos.Add(dbInstallationInfo);
      }

      _context.SaveChanges();

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyApiDownloader: _downloaderMock.Object);

      // Act
      await programController.UpdateInstallationInfos(newInstallationInfos);

      var dbInstallationInfos
          = _context.InstallationInfos.OrderBy(i => i.InstallationId).ToList();

      // Assert
      Assert.Equal(_installationIds.Count, dbInstallationInfos.Count);

      for (int i = 0; i < dbInstallationInfos.Count; i++)
      {
        Assert.Equal(_installationIds[i], dbInstallationInfos[i].InstallationId);
      }
    }

    [Fact]
    public async Task removes_installation_info_objects_from_database_for_not_requested_installations()
    {
      // Arrange
      var newInstallationIds = _installationIds.SkipLast(1).ToList();
      var newInstallationInfos = new List<InstallationInfo>();

      foreach (short installationId in _installationIds)
      {
        var dbInstallationInfo = GetTestInstallationInfo(
            installationId, new DateTime());

        _context.InstallationInfos.Add(dbInstallationInfo);
      }

      _context.SaveChanges();

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: newInstallationIds,
          airlyApiDownloader: _downloaderMock.Object);

      // Act
      await programController.UpdateInstallationInfos(newInstallationInfos);

      var dbInstallationInfos
          = _context.InstallationInfos.OrderBy(i => i.InstallationId).ToList();

      // Assert
      Assert.Equal(newInstallationIds.Count, dbInstallationInfos.Count);

      for (int i = 0; i < dbInstallationInfos.Count; i++)
      {
        Assert.Equal(newInstallationIds[i], dbInstallationInfos[i].InstallationId);
      }
    }

    [Fact]
    public async Task updates_installation_infos()
    {
      // Arrange
      var oldUpdateDate = new DateTime();
      var newUpdateDate = oldUpdateDate.AddDays(7);
      var newInstallationInfos = new List<InstallationInfo>();

      foreach (short installationId in _installationIds)
      {
        var dbInstallationInfo
            = GetTestInstallationInfo(installationId, oldUpdateDate);

        _context.InstallationInfos.Add(dbInstallationInfo);
      }

      _context.SaveChanges();

      foreach (short installationId in _installationIds)
      {
        var installationInfo
            = GetTestInstallationInfo( installationId, newUpdateDate);

        newInstallationInfos.Add(installationInfo);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyApiDownloader: _downloaderMock.Object);

      // Act
      await programController.UpdateInstallationInfos(newInstallationInfos);

      var dbInstallationInfos
          = _context.InstallationInfos.OrderBy(i => i.InstallationId).ToList();

      // Assert
      Assert.Equal(_installationIds.Count, dbInstallationInfos.Count);

      for (int i = 0; i < dbInstallationInfos.Count; i++)
      {
        Assert.Equal(_installationIds[i], dbInstallationInfos[i].InstallationId);
        Assert.Equal(newUpdateDate, dbInstallationInfos[i].UpdateDate);
      }
    }

    [Fact]
    public async Task add_installation_info_objects_from_new_installations()
    {
      // Arrange
      var newInstallationInfos = new List<InstallationInfo>();
      var newInstallationIds = new List<short>
      {
        (short)(_installationIds.Last() + 1),
        (short)(_installationIds.Last() + 2)
      };

      foreach (short installationId in _installationIds)
      {
        var dbInstallationInfo = GetTestInstallationInfo(
            installationId, new DateTime());

        _context.InstallationInfos.Add(dbInstallationInfo);
      }

      _context.SaveChanges();

      foreach (short installationId in newInstallationIds)
      {
        var installationInfo = GetTestInstallationInfo(
            installationId, new DateTime());

        newInstallationInfos.Add(installationInfo);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: newInstallationIds,
          airlyApiDownloader: _downloaderMock.Object);

      // Act
      await programController.UpdateInstallationInfos(newInstallationInfos);

      var dbInstallationInfos
          = _context.InstallationInfos.OrderBy(i => i.InstallationId).ToList();

      // Assert
      Assert.Equal(newInstallationIds.Count, dbInstallationInfos.Count);

      for (int i = 0; i < dbInstallationInfos.Count; i++)
      {
        Assert.Equal(newInstallationIds[i], dbInstallationInfos[i].InstallationId);
      }
    }
  }
}
