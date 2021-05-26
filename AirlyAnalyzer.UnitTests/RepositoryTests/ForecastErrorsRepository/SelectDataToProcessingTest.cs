namespace AirlyAnalyzer.UnitTests.RepositoryTests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Xunit;

  [Collection("RepositoryTests")]
  public class SelectDataToProcessingTest
  {
    private readonly AirlyContext _context;
    private readonly UnitOfWork _unitOfWork;

    private readonly DateTime _startDate;

    private readonly List<short> _installationIds;

    public SelectDataToProcessingTest(RepositoryFixture fixture)
    {
      _context = fixture.Context;
      _unitOfWork = fixture.UnitOfWork;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      _context.Clear();
    }

    [Fact]
    public async Task returns_empty_new_data_when_no_data_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];

      // Act
      var (newMeasurements, newForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(
              installationId, ForecastErrorClass.Plain, AirQualityDataSource.Airly);

      // Assert
      Assert.Empty(newMeasurements);
      Assert.Empty(newForecasts);
    }

    [Fact]
    public async Task returns_empty_new_data_when_no_data_to_process_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfProcessedDays = 5;
      const short numberOfElementsInDay = 24;

      _context.AddAllElementsToDatabase(
          _installationIds,
          _startDate,
          numberOfProcessedDays,
          numberOfElementsInDay);

      // Act
      var (newMeasurements, newForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(
              installationId, ForecastErrorClass.Plain, AirQualityDataSource.Airly);

      // Assert
      Assert.Empty(newMeasurements);
      Assert.Empty(newForecasts);
    }

    [Fact]
    public async Task returns_new_data_when_only_data_to_process_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfNotProcessedDays = 1;
      const short numberOfNewMeasurementsInDay = 24;
      const short numberOfNewForecastsInDay = 24;

      var newMeasurementsStartDate = _startDate;
      var newForecastsStartDate = _startDate;

      _context.AddAllMeasurementsToDatabase(
          _installationIds,
          newMeasurementsStartDate,
          numberOfNotProcessedDays,
          numberOfNewMeasurementsInDay);

      _context.AddAllForecastsToDatabase(
          _installationIds,
          newForecastsStartDate,
          numberOfNotProcessedDays,
          numberOfNewForecastsInDay);

      // Act
      var (newMeasurements, newForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(
              installationId, ForecastErrorClass.Plain, AirQualityDataSource.Airly);

      // Assert
      Assert.Equal(
          numberOfNewMeasurementsInDay * numberOfNotProcessedDays,
          newMeasurements.Count);
      Assert.Equal(
          numberOfNewForecastsInDay * numberOfNotProcessedDays,
          newForecasts.Count);
    }

    [Fact]
    public async Task returns_new_forecasts_from_correct_source()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfNotProcessedDays = 1;
      const short numberOfElementsInDay = 24;
      const short numberOfNewAppForecastsInDay = 19;

      var newElementsStartDate = _startDate;

      _context.AddAllForecastsToDatabase(
          _installationIds,
          newElementsStartDate,
          numberOfNotProcessedDays,
          numberOfElementsInDay,
          AirQualityDataSource.Airly);

      _context.AddAllForecastsToDatabase(
          _installationIds,
          newElementsStartDate,
          numberOfNotProcessedDays,
          numberOfNewAppForecastsInDay,
          AirQualityDataSource.App);

      // Act
      var (newMeasurements, newForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(
              installationId, ForecastErrorClass.Plain, AirQualityDataSource.App);

      // Assert
      Assert.Equal(
          numberOfNewMeasurementsInDay * numberOfNotProcessedDays,
          newMeasurements.Count);
      Assert.Equal(
          numberOfNewAppForecastsInDay * numberOfNotProcessedDays,
          newForecasts.Count);
    }

    [Theory]
    [InlineData(1, 2, 4)]
    [InlineData(2, 23, 24)]
    [InlineData(2, 24, 23)]
    public async Task returns_new_data_when_data_to_process_from_several_installations_in_database(
        short numberOfNotProcessedDays,
        short numberOfNewMeasurementsInDay,
        short numberOfNewForecastsInDay)
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfProcessedDays = 5;
      const short numberOfElementsInDay = 23;
      var processedDataStartDate = _startDate;
      var newMeasurementsStartDate = _startDate.AddDays(numberOfProcessedDays);
      var newForecastsStartDate = _startDate.AddDays(numberOfProcessedDays);

      _context.AddAllElementsToDatabase(
          _installationIds,
          processedDataStartDate,
          numberOfProcessedDays,
          numberOfElementsInDay);

      _context.AddAllMeasurementsToDatabase(
          _installationIds,
          newMeasurementsStartDate,
          numberOfNotProcessedDays,
          numberOfNewMeasurementsInDay);

      _context.AddAllForecastsToDatabase(
          _installationIds,
          newForecastsStartDate,
          numberOfNotProcessedDays,
          numberOfNewForecastsInDay);

      // Act
      var (newMeasurements, newForecasts) = await _unitOfWork
          .ForecastErrorRepository.SelectDataToProcessing(
              installationId, ForecastErrorClass.Plain, AirQualityDataSource.Airly);

      // Assert
      Assert.Equal(
          numberOfNewMeasurementsInDay * numberOfNotProcessedDays,
          newMeasurements.Count);
      Assert.Equal(
          numberOfNewForecastsInDay * numberOfNotProcessedDays,
          newForecasts.Count);
      Assert.Equal(installationId, newMeasurements[0].InstallationId);
      Assert.Equal(installationId, newForecasts[0].InstallationId);
    }
  }
}
