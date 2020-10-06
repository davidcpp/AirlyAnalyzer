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
    private byte _numberOfMeasurements;
    private short _numberOfDays = 25;
    private short _numberOfMeasurementsInDay = 23;

    [Fact]
    public void Return_valid_last_DateTime_when_one_day_measurements()
    {
      // Arrange
      var endDate = _startDate.AddDays(1);
      _numberOfMeasurements = 24;

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, _numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate, measurements.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_list_of_measurements_from_one_day()
    {
      // Arrange
      _numberOfMeasurements = 20;

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, _numberOfMeasurements, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(_numberOfMeasurements, measurements.Count);
    }

    [Fact]
    public void Return_list_of_measurements_from_many_days()
    {
      // Arrange 
      _numberOfDays = 25;
      _numberOfMeasurementsInDay = 23;

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, _numberOfDays, _numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(_numberOfDays * _numberOfMeasurementsInDay, measurements.Count);
    }

    [Fact]
    public void Return_valid_last_measurement_date_when_many_days_measurements()
    {
      // Arrange 
      _numberOfDays = 25;
      _numberOfMeasurementsInDay = 23;
      var endDate = _startDate.AddHours(_numberOfDays * _numberOfMeasurementsInDay);

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, _numberOfDays, _numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endDate, measurements.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void Return_valid_last_request_date_when_many_days_measurements()
    {
      // Arrange 
      _numberOfDays = 25;
      _numberOfMeasurementsInDay = 23;
      var endRequestDate = _startDate.AddHours(_numberOfDays * _numberOfMeasurementsInDay)
                                     .AddMinutes(_requestMinutesOffset);

      // Act
      var measurements = AuxiliaryMethods
        .GenerateMeasurements(_startDate, _numberOfDays, _numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      // Assert
      Assert.Equal(endRequestDate, measurements.Last().RequestDateTime, new TimeSpan(0, 0, 0));
    }
  }
}
