using System;
using System.Linq;
using AirlyAnalyzer.Tests.Models;
using Xunit;

namespace AirlyAnalyzer.Tests
{
  public class GenerateMeasurementsTest
  {
    private readonly DateTime _startDate = new DateTime(2001, 3, 15, 22, 0, 0, DateTimeKind.Local);
    private const byte _requestMinutesOffset = 30;

    [Fact]
    public void Return_list_of_measurements_from_one_day()
    {
      // Arrange
      const int numberOfMeasurements = 20;

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfMeasurements, measurements.Count);
    }

    [Fact]
    public void Return_correct_last_DateTime_of_measurements_from_one_day()
    {
      // Arrange
      var endDate = _startDate.AddDays(1);
      const int numberOfMeasurements = 24;

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate, measurements.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_list_of_measurements_from_many_days()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfMeasurementsInDay = 23;

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(numberOfDays * numberOfMeasurementsInDay, measurements.Count);
    }

    [Fact]
    public void Return_correct_last_measurement_date_when_many_days_measurements()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfMeasurementsInDay = 23;
      var endDate = _startDate.AddHours(numberOfDays * numberOfMeasurementsInDay);

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate, measurements.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_correct_last_request_date_when_many_days_measurements()
    {
      // Arrange 
      const short numberOfDays = 25;
      const short numberOfMeasurementsInDay = 23;
      var endRequestDate = _startDate.AddHours(numberOfDays * numberOfMeasurementsInDay)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate, measurements.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }
  }
}
