namespace AirlyAnalyzer.Tests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Tests.Models;
  using Xunit;

  public class GenerateMeasurementsTest
  {
    private readonly DateTime _startDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);
    private const byte _requestMinutesOffset = 30;
    private const short _installationId = 1;

    [Fact]
    public void list_of_measurements_from_one_day()
    {
      // Arrange
      const int numberOfMeasurements = 20;

      // Act
      var measurements = AuxiliaryMethods.GenerateMeasurements(
          _installationId, _startDate, numberOfMeasurements, _requestMinutesOffset)
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
      var measurements = AuxiliaryMethods.GenerateMeasurements(
          _installationId, _startDate, numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), measurements.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void list_of_measurements_from_many_days()
    {
      // Arrange
      const short numberOfDays = 25;
      const short numberOfMeasurementsInDay = 23;

      // Act
      var measurements = AuxiliaryMethods.GenerateMeasurements(
          _installationId, _startDate, numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfDays * numberOfMeasurementsInDay, measurements.Count);
    }

    [Fact]
    public void correct_end_date_of_measurements_from_many_days()
    {
      // Arrange
      const short numberOfDays = 25;
      const short numberOfMeasurementsInDay = 23;
      var endDate = _startDate.AddDays(numberOfDays)
                              .AddHours(numberOfMeasurementsInDay - 24);

      // Act
      var measurements = AuxiliaryMethods.GenerateMeasurements(
          _installationId, _startDate, numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate.ToLocalTime(), measurements.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void correct_last_request_date_of_measurements_from_many_days()
    {
      // Arrange
      const short numberOfDays = 25;
      const short numberOfMeasurementsInDay = 23;
      var endRequestDate = _startDate.AddDays(numberOfDays)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var measurements = AuxiliaryMethods.GenerateMeasurements(
          _installationId, _startDate, numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate.ToLocalTime(), measurements.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }
  }
}
