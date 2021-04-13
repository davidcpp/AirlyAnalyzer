namespace AirlyAnalyzer.Calculation
{
  using System.Collections.Generic;
  using AirlyAnalyzer.Models;

  public interface IForecastErrorsCalculator
  {
    public ForecastErrorClass ErrorClass { get; }

    List<AirQualityForecastError> CalculateHourly(
        short installationId,
        List<AirQualityMeasurement> newMeasurements,
        List<AirQualityForecast> newForecasts);

    List<AirQualityForecastError> CalculateDaily(
        short installationId,
        short minNumberOfMeasurements,
        List<AirQualityForecastError> newHourlyForecastErrors);

    AirQualityForecastError CalculateTotal(
        short installationId, IEnumerable<AirQualityForecastError> allForecastErrors);
  }
}
