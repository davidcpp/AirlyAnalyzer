namespace AirlyAnalyzer.Tests.ForecastErrorCalculationTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Tests.Models;
  using Xunit;

  public class CalculateNewForecastErrorsTest
  {
    private readonly DateTime _startDate;
    private const byte _requestMinutesOffset = 30;

    public CalculateNewForecastErrorsTest()
    {
      _startDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Local);
    }

    [Fact]
    public void correct_forecast_error_list_when_overlapping_elements()
    {
      // Arrange
      const short installationId = 1;
      const short minNumberOfMeasurements = 23;
      const short numberOfDays = 2;
      const short numberOfMeasurementsInDay = 24;
      const short numberOfForecastsInDay = 24;
      var measurementsStartDate = _startDate;
      var forecastsStartDate = _startDate.AddDays(1);
      var measurementsEndDate = _startDate.AddDays(2);

      var newMeasurements = AuxiliaryMethods.GenerateMeasurements(
          installationId, measurementsStartDate,numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset)
        .ToList();

      var newForecasts = AuxiliaryMethods.GenerateForecasts(
          installationId, forecastsStartDate, numberOfDays, numberOfForecastsInDay, _requestMinutesOffset)
        .ToList();

      var forecastErrorsCalculation = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var forecastErrors = forecastErrorsCalculation
        .CalculateNewForecastErrors(installationId, newMeasurements, newForecasts);

      var dailyForecastErrors = forecastErrors.Where(e => e.ErrorType == ForecastErrorType.Daily);

      // Assert
      Assert.Single(dailyForecastErrors);
      Assert.Equal(forecastsStartDate, dailyForecastErrors.First().FromDateTime, new TimeSpan(0, 0, 0));
      Assert.Equal(measurementsEndDate, dailyForecastErrors.First().TillDateTime, new TimeSpan(0, 0, 0));
      // _minNumberOfMeasurements hourly forecast errors + 1 daily forecast error
      Assert.Equal(numberOfMeasurementsInDay + 1, forecastErrors.Count);
      Assert.Equal(forecastsStartDate, forecastErrors[0].FromDateTime, new TimeSpan(0, 0, 0));
      Assert.Equal(measurementsEndDate, forecastErrors.Last().TillDateTime, new TimeSpan(0, 0, 0));
    }

    [Fact]
    public void empty_forecast_error_list_when_non_overlapping_elements()
    {
      // Arrange
      const short installationId = 1;
      const short minNumberOfMeasurements = 23;
      const short numberOfMeasurements = 24;
      const short numberOfForecasts = 24;
      // These start date values simulate a case of a two-day break in requests 
      // - future measurements begin where previous forecasts end
      var measurementsStartDate = _startDate.AddHours(numberOfMeasurements);
      var forecastsStartDate = _startDate;

      var newMeasurements = AuxiliaryMethods.GenerateMeasurements(
        installationId, measurementsStartDate, numberOfMeasurements, _requestMinutesOffset).ToList();

      var newForecasts = AuxiliaryMethods.GenerateForecasts(
        installationId, forecastsStartDate, numberOfForecasts, _requestMinutesOffset).ToList();

      var forecastErrorsCalculation = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var forecastErrors = forecastErrorsCalculation
        .CalculateNewForecastErrors(installationId, newMeasurements, newForecasts);

      // Assert
      Assert.Empty(forecastErrors);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0)]
    [InlineData(0, 0, 0, 2, 2, 2, -200, -200, -200)]
    [InlineData(0, 0, 0, 255, 240, 320, -25500, -24000, -32000)]
    public void negative_forecast_errors_when_measurements_are_zero(
      byte airlyCaqi_Measurement, short airlyPm25_Measurement, short airlyPm10_Measurement,
      byte airlyCaqi_Forecast, short airlyPm25_Forecast, short airlyPm10_Forecast,
      short airlyCaqi_ForecastError, short airlyPm25_ForecastError, short airlyPm10_ForecastError)
    {
      // Arrange
      const short installationId = 1;
      const short minNumberOfMeasurements = 23;
      var measurementsStartDate = _startDate;
      var measurementsRequestDate = _startDate.AddDays(1).AddMinutes(_requestMinutesOffset);
      var forecastsStartDate = _startDate;
      var forecastsRequestDate = _startDate;

      var measurement = AuxiliaryMethods.CreateMeasurement(installationId, measurementsStartDate,
        measurementsRequestDate, airlyCaqi_Measurement, airlyPm25_Measurement, airlyPm10_Measurement);
      var forecast = AuxiliaryMethods.CreateForecast(installationId, forecastsStartDate,
        forecastsRequestDate, airlyCaqi_Forecast, airlyPm25_Forecast, airlyPm10_Forecast);

      var newMeasurements = new List<AirQualityMeasurement> { measurement };
      var newForecasts = new List<AirQualityForecast> { forecast };

      var forecastErrorsCalculation = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var forecastErrors = forecastErrorsCalculation
        .CalculateNewForecastErrors(installationId, newMeasurements, newForecasts);

      // Assert
      Assert.Single(forecastErrors);
      Assert.Equal(airlyCaqi_ForecastError, forecastErrors[0].AirlyCaqiPctError);
      Assert.Equal(airlyPm25_ForecastError, forecastErrors[0].Pm25PctError);
      Assert.Equal(airlyPm10_ForecastError, forecastErrors[0].Pm10PctError);
    }

    [Theory]
    [InlineData(1, 1, 1, 255, 240, 320, -25400, -23900, -31900)] // min non-zero measurements and max forecasts
                                                                 // -> max errors
    [InlineData(255, 240, 320, 254, 239, 319, 0, 0, 0)] // min positive diff between measurements and forecasts
                                                        //-> zero errors
    [InlineData(254, 239, 319, 255, 240, 320, 0, 0, 0)] // min negative diff between measurements and forecasts
                                                        //-> zero errors
    [InlineData(50, 30, 45, 50, 30, 45, 0, 0, 0)] // measurements equal forecasts -> zero errors
    [InlineData(50, 30, 45, 0, 0, 0, 100, 100, 100)] // random measurements, zero forecast values -> max errors
    [InlineData(50, 30, 45, 49, 29, 44, 2, 3, 2)] // random values and minimum positive error
    [InlineData(40, 40, 40, 39, 39, 39, 2, 2, 2)] // test rounding positive to even numbers (2.5->2)
    [InlineData(40, 40, 40, 41, 41, 41, -2, -2, -2)] // test rounding negative errors to even numbers (2.5->2)
    [InlineData(200, 180, 240, 197, 175, 235, 2, 3, 2)] // test round positive errors to even numbers (1.5->2)
    [InlineData(200, 180, 240, 203, 185, 245, -2, -3, -2)] // test round negative errors to even numbers (1.5->2)
    public void correctly_calculated_forecast_errors(
      byte airlyCaqi_Measurement, short airlyPm25_Measurement, short airlyPm10_Measurement,
      byte airlyCaqi_Forecast, short airlyPm25_Forecast, short airlyPm10_Forecast,
      short airlyCaqi_ForecastError, short airlyPm25_ForecastError, short airlyPm10_ForecastError)
    {
      // Arrange
      const short installationId = 1;
      const short minNumberOfMeasurements = 23;
      var measurementsStartDate = _startDate;
      var measurementsRequestDate = _startDate.AddDays(1).AddMinutes(_requestMinutesOffset);
      var forecastsStartDate = _startDate;
      var forecastsRequestDate = _startDate;

      var measurement = AuxiliaryMethods.CreateMeasurement(installationId, measurementsStartDate,
        measurementsRequestDate, airlyCaqi_Measurement, airlyPm25_Measurement, airlyPm10_Measurement);
      var forecast = AuxiliaryMethods.CreateForecast(installationId, forecastsStartDate,
        forecastsRequestDate, airlyCaqi_Forecast, airlyPm25_Forecast, airlyPm10_Forecast);

      var newMeasurements = new List<AirQualityMeasurement> { measurement };
      var newForecasts = new List<AirQualityForecast> { forecast };

      var forecastErrorsCalculation = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var forecastErrors = forecastErrorsCalculation
        .CalculateNewForecastErrors(installationId, newMeasurements, newForecasts);

      // Assert
      Assert.Single(forecastErrors);
      Assert.Equal(airlyCaqi_ForecastError, forecastErrors[0].AirlyCaqiPctError);
      Assert.Equal(airlyPm25_ForecastError, forecastErrors[0].Pm25PctError);
      Assert.Equal(airlyPm10_ForecastError, forecastErrors[0].Pm10PctError);
    }

    [Fact]
    public void no_daily_forecast_error_when_number_of_elements_less_than_min()
    {
      // Arrange
      const short installationId = 1;
      const short minNumberOfMeasurements = 20;
      const short numberOfMeasurements = 19;
      const short numberOfForecasts = 19;
      var measurementsStartDate = _startDate;
      var forecastsStartDate = _startDate;

      var newMeasurements = AuxiliaryMethods.GenerateMeasurements(
        installationId, measurementsStartDate, numberOfMeasurements, _requestMinutesOffset).ToList();

      var newForecasts = AuxiliaryMethods.GenerateForecasts(
        installationId, forecastsStartDate, numberOfForecasts, _requestMinutesOffset).ToList();

      var forecastErrorsCalculation = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var dailyForecastErrors = forecastErrorsCalculation
        .CalculateNewForecastErrors(installationId, newMeasurements, newForecasts)
        .Where(e => e.ErrorType == ForecastErrorType.Daily);

      // Assert
      Assert.Empty(dailyForecastErrors);
    }

    [Fact]
    public void correct_daily_forecast_error_when_number_of_elements_is_equal_min()
    {
      // Arrange
      const short installationId = 1;
      const short minNumberOfMeasurements = 20;
      const short numberOfMeasurements = 20;
      const short numberOfForecasts = 20;
      var measurementsStartDate = _startDate;
      var forecastsStartDate = _startDate;
      var endDate = _startDate.AddHours(numberOfMeasurements);

      var newMeasurements = AuxiliaryMethods.GenerateMeasurements(
        installationId, measurementsStartDate, numberOfMeasurements, _requestMinutesOffset).ToList();

      var newForecasts = AuxiliaryMethods.GenerateForecasts(
        installationId, forecastsStartDate, numberOfForecasts, _requestMinutesOffset).ToList();

      var forecastErrorsCalculation = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var dailyForecastErrors = forecastErrorsCalculation
        .CalculateNewForecastErrors(installationId, newMeasurements, newForecasts)
        .Where(e => e.ErrorType == ForecastErrorType.Daily);

      // Assert
      Assert.Equal(forecastsStartDate, dailyForecastErrors.First().FromDateTime, new TimeSpan(0, 0, 0));
      Assert.Equal(endDate, dailyForecastErrors.First().TillDateTime, new TimeSpan(0, 0, 0));
      Assert.Single(dailyForecastErrors);
    }

    [Theory]
    [InlineData(1, 22, 22, 22, 23, 1)]
    [InlineData(1, 21, 21, 22, 21, 0)]
    [InlineData(1, 21, 21, 18, 22, 1)]
    [InlineData(2, 22, 22, 22, 46, 2)]
    [InlineData(2, 21, 21, 22, 42, 0)]
    [InlineData(13, 22, 22, 22, 299, 13)]
    [InlineData(13, 21, 21, 22, 273, 0)]
    public void correct_number_of_forecast_errors(
      short numberOfDays,
      short numberOfMeasurementsInDay,
      short numberOfForecastsInDay,
      short minNumberOfMeasurements,
      int numberOfForecastsErrors,
      short numberOfDailyForecastsErrors)
    {
      // Arrange
      const short installationId = 1;
      var measurementsStartDate = _startDate;
      var forecastsStartDate = _startDate;

      var newMeasurements = AuxiliaryMethods.GenerateMeasurements(installationId, measurementsStartDate,
        numberOfDays, numberOfMeasurementsInDay, _requestMinutesOffset).ToList();

      var newForecasts = AuxiliaryMethods.GenerateForecasts(installationId, forecastsStartDate,
        numberOfDays, numberOfForecastsInDay, _requestMinutesOffset).ToList();

      var forecastErrorsCalculation = new ForecastErrorsCalculation(minNumberOfMeasurements);

      // Act
      var forecastErrors = forecastErrorsCalculation
        .CalculateNewForecastErrors(installationId, newMeasurements, newForecasts);

      var dailyForecastErrors = forecastErrors.Where(e => e.ErrorType == ForecastErrorType.Daily);

      // Assert
      Assert.Equal(numberOfForecastsErrors, forecastErrors.Count);
      Assert.Equal(numberOfDailyForecastsErrors, dailyForecastErrors.Count());
    }
  }
}
