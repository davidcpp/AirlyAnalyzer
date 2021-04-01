namespace AirlyAnalyzer.Tests.AuxiliaryMethodsTests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Tests.Fixtures;
  using AirlyAnalyzer.Tests.Helpers;
  using Xunit;

  public class GenerateMeasurementsTest : IClassFixture<SimpleFixture>
  {
    private readonly short _installationId;
    private readonly DateTime _startDate;

    public GenerateMeasurementsTest(SimpleFixture fixture)
    {
      _startDate = fixture.StartDate;
      _installationId = fixture.InstallationId;
    }

    [Fact]
    public void returns_list_of_measurements_from_one_day()
    {
      // Arrange
      const int numberOfMeasurements = 20;

      // Act
      var measurements = ModelUtilities.GenerateMeasurements(
          _installationId, _startDate, numberOfMeasurements)
        .ToList();

      // Assert
      Assert.Equal(numberOfMeasurements, measurements.Count);
    }

    [Fact]
    public void correct_end_date_of_measurements_from_one_day()
    {
      // Arrange
      const int numberOfMeasurements = 24;
      var endDate = _startDate.AddDays(1);

      // Act
      var measurements = ModelUtilities.GenerateMeasurements(
          _installationId, _startDate, numberOfMeasurements)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), measurements.Last().TillDateTime);
    }

    [Fact]
    public void returns_list_of_measurements_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfMeasurementsInDay = 23;

      // Act
      var measurements = ModelUtilities.GenerateMeasurements(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfMeasurementsInDay)
        .ToList();

      // Assert
      Assert.Equal(
          numberOfDays * numberOfMeasurementsInDay,
          measurements.Count);
    }

    [Fact]
    public void correct_end_date_of_measurements_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfMeasurementsInDay = 23;
      var endDate = _startDate.AddDays(numberOfDays)
                              .AddHours(numberOfMeasurementsInDay - 24);

      // Act
      var measurements = ModelUtilities.GenerateMeasurements(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfMeasurementsInDay)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), measurements.Last().TillDateTime);
    }

    [Fact]
    public void correct_last_request_date_of_measurements_from_many_days()
    {
      // Arrange
      const short numberOfDays = 15;
      const short numberOfMeasurementsInDay = 23;
      var endRequestDate = _startDate
          .AddDays(numberOfDays)
          .AddMinutes(ModelUtilities.RequestMinutesOffset);

      // Act
      var measurements = ModelUtilities.GenerateMeasurements(
          _installationId,
          _startDate,
          numberOfDays,
          numberOfMeasurementsInDay)
        .ToList();

      // Assert
      Assert.Equal(
          endRequestDate.ToLocalTime(),
          measurements.Last().RequestDateTime);
    }
  }
}
