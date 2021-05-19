namespace AirlyAnalyzer.UnitTests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Microsoft.Extensions.Configuration;
  using Moq;
  using Xunit;

  [Collection("RepositoryTests")]
  public class DownloadAllAirQualityDataTest
  {
    private readonly Mock<IAirlyApiDownloader> _downloaderMock;

    private readonly UnitOfWork _unitOfWork;
    private readonly AirlyContext _context;
    private readonly List<short> _installationIds;
    private readonly short _minNumberOfMeasurements;

    public DownloadAllAirQualityDataTest(RepositoryFixture fixture)
    {
      _unitOfWork = fixture.UnitOfWork;
      _context = fixture.Context;
      _installationIds = fixture.InstallationIds;

      _minNumberOfMeasurements = fixture.Config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _context.Clear();

      _downloaderMock = new Mock<IAirlyApiDownloader>();
    }

    [Fact]
    public async Task does_not_download_when_no_installations()
    {
      // Arrange
      const short numberOfDays = 1;

      var startDate = DateTime.UtcNow.AddHours(-(2 * _minNumberOfMeasurements));
      var downloadedData = new Measurements();
      var emptyInstallationIds = new List<short>();

      _context.AddAllMeasurementsToDatabase(
          _installationIds, startDate, numberOfDays, _minNumberOfMeasurements);

      _downloaderMock
          .Setup(x => x.DownloadInstallationMeasurements(It.IsAny<short>()))
          .ReturnsAsync(downloadedData);

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: emptyInstallationIds,
          airlyApiDownloader: _downloaderMock.Object,
          minNumberOfMeasurements: _minNumberOfMeasurements);

      // Act
      var newMeasurementsList
          = await programController.DownloadAllAirQualityData();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadInstallationMeasurements(It.IsAny<short>()),
          Times.Never());

      Assert.Empty(newMeasurementsList);
    }

    [Theory]
    [InlineData(22)]
    [InlineData(24)]
    public async Task does_not_download_when_data_is_up_to_date(
        short minNumberOfMeasurements)
    {
      // Arrange
      const short numberOfDays = 1;

      var downloadedData = new Measurements();
      var startDate = DateTime.UtcNow.AddHours(-(2 * minNumberOfMeasurements) + 1);

      _context.AddAllMeasurementsToDatabase(
          _installationIds, startDate, numberOfDays, minNumberOfMeasurements);

      _downloaderMock
          .Setup(x => x.DownloadInstallationMeasurements(It.IsAny<short>()))
          .ReturnsAsync(downloadedData);

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyApiDownloader: _downloaderMock.Object,
          minNumberOfMeasurements: minNumberOfMeasurements);

      // Act
      var newMeasurementsList
          = await programController.DownloadAllAirQualityData();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadInstallationMeasurements(It.IsAny<short>()),
          Times.Never());

      Assert.Equal(_installationIds.Count, newMeasurementsList.Count);
    }

    [Fact]
    public async Task downloads_for_all_installations_when_no_measurements_in_database()
    {
      // Arrange
      var downloadedData = new Measurements();

      foreach (short installationId in _installationIds)
      {
        _downloaderMock.Setup(x => x.DownloadInstallationMeasurements(installationId))
                       .ReturnsAsync(downloadedData);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyApiDownloader: _downloaderMock.Object);

      // Act
      var newMeasurementsList
          = await programController.DownloadAllAirQualityData();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadInstallationMeasurements(It.IsAny<short>()),
          Times.Exactly(_installationIds.Count));

      Assert.Equal(_installationIds.Count, newMeasurementsList.Count);
    }

    [Theory]
    [InlineData(22)]
    [InlineData(24)]
    public async Task downloads_for_all_installations_when_data_is_out_of_date(
        short minNumberOfMeasurements)
    {
      // Arrange
      const short numberOfDays = 1;

      var downloadedData = new Measurements();

      var startDate = DateTime.UtcNow.AddHours(-(2 * minNumberOfMeasurements));

      _context.AddAllMeasurementsToDatabase(
          _installationIds, startDate, numberOfDays, minNumberOfMeasurements);

      foreach (short installationId in _installationIds)
      {
        _downloaderMock.Setup(
            x => x.DownloadInstallationMeasurements(installationId))
                       .ReturnsAsync(downloadedData);
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyApiDownloader: _downloaderMock.Object,
          minNumberOfMeasurements: minNumberOfMeasurements);

      // Act
      var newMeasurementsList
          = await programController.DownloadAllAirQualityData();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadInstallationMeasurements(It.IsAny<short>()),
          Times.Exactly(_installationIds.Count));

      Assert.Equal(_installationIds.Count, newMeasurementsList.Count);
    }

    [Fact]
    public async Task downloads_for_some_installations_when_some_data_is_out_of_date()
    {
      // Arrange
      const short numberOfDays = 1;

      var downloadedData = new Measurements();
      var installationIds = new List<short>();

      var startDate = DateTime.UtcNow.AddHours(-(2 * _minNumberOfMeasurements));

      for (int i = 0; i < _installationIds.Count; i++)
      {
        _context.AddMeasurementsToDatabase(
            _installationIds[i], startDate, numberOfDays, _minNumberOfMeasurements);

        if (i % 2 == 0)
        {
          _context.AddMeasurementsToDatabase(
              _installationIds[i],
              startDate.AddHours(_minNumberOfMeasurements),
              numberOfDays,
              _minNumberOfMeasurements);
        }
        else
        {
          _downloaderMock.Setup(
              x => x.DownloadInstallationMeasurements(_installationIds[i]))
                         .ReturnsAsync(downloadedData);
        }
      }

      var programController = new ProgramController(
          unitOfWork: _unitOfWork,
          installationIds: _installationIds,
          airlyApiDownloader: _downloaderMock.Object,
          minNumberOfMeasurements: _minNumberOfMeasurements);

      // Act
      var newMeasurementsList
          = await programController.DownloadAllAirQualityData();

      // Assert
      _downloaderMock.Verify(
          x => x.DownloadInstallationMeasurements(It.IsAny<short>()),
          Times.Exactly((int)Math.Ceiling((double)_installationIds.Count / 2)));

      Assert.Equal(_installationIds.Count, newMeasurementsList.Count);
    }
  }
}
