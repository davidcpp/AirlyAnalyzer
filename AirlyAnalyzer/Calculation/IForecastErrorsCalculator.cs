﻿namespace AirlyAnalyzer.Calculation
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Models;

  public interface IForecastErrorsCalculator
  {
    public List<AirQualityForecastError> CalculateHourly(
        short installationId,
        List<AirQualityMeasurement> newArchiveMeasurements,
        List<AirQualityForecast> newArchiveForecasts);

    public List<AirQualityForecastError> CalculateDaily(
        short installationId,
        short minNumberOfMeasurements,
        List<AirQualityForecastError> newHourlyForecastErrors);

    public AirQualityForecastError CalculateTotal(
        short installationId, IEnumerable<AirQualityForecastError> allForecastErrors);
  }
}
