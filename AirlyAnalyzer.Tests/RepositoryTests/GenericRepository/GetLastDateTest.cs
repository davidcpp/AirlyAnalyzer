namespace AirlyAnalyzer.Tests.RepositoryTests
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Tests.Fixtures;
  using AirlyAnalyzer.Tests.Helpers;
  using Xunit;

  [Collection("RepositoryTests")]
  public class GetLastDateTest
  {
    private readonly AirlyContext _context;
    private readonly UnitOfWork _unitOfWork;

    private readonly DateTime _dateTimeMinValue = new DateTime(2000, 1, 1);
    private readonly DateTime _startDate;

    private readonly List<short> _installationIds;

    public GetLastDateTest(RepositoryFixture fixture)
    {
      _context = fixture.Context;
      _unitOfWork = fixture.UnitOfWork;
      _dateTimeMinValue = fixture.DateTimeMinValue;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      _context.Clear();
    }

    [Fact]
    public async void returns_date_time_min_value_when_no_data_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];

      // Act
      var lastMeasurementDate = await _unitOfWork
          .MeasurementRepository.GetLastDate(installationId);

      // Assert
      Assert.Equal(_dateTimeMinValue, lastMeasurementDate);
    }

    [Fact]
    public async void returns_date_time_min_value_when_only_data_from_other_installations_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfMeasurements = 23;
      var measurementsStartDate = _startDate;

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        _context.AddMeasurementsToDatabase(
            _installationIds[i], measurementsStartDate, numberOfMeasurements);
      }

      // Act
      var lastMeasurementDate = await _unitOfWork
          .MeasurementRepository.GetLastDate(installationId);

      // Assert
      Assert.Equal(_dateTimeMinValue, lastMeasurementDate);
    }

    [Fact]
    public async void returns_correct_date_when_all_installations_data_in_database()
    {
      // Arrange
      short installationId = _installationIds[0];
      const short numberOfMeasurements = 22;
      const short numberOfOtherMeasurements = 23;

      var measurementsStartDate = _startDate;
      var properDate = _startDate.AddHours(numberOfMeasurements);

      _context.AddMeasurementsToDatabase(
          installationId, measurementsStartDate, numberOfMeasurements);

      // all installations except the selected
      for (int i = 1; i < _installationIds.Count; i++)
      {
        _context.AddMeasurementsToDatabase(
            _installationIds[i], measurementsStartDate, numberOfOtherMeasurements);
      }

      // Act
      var lastMeasurementDate = await _unitOfWork
          .MeasurementRepository.GetLastDate(installationId);

      // Assert
      Assert.Equal(properDate, lastMeasurementDate);
    }
  }
}
