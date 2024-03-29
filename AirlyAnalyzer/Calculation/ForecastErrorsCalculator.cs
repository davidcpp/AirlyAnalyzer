﻿namespace AirlyAnalyzer.Calculation
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;

  public abstract class ForecastErrorsCalculator : IForecastErrorsCalculator
  {
    protected short _idForAllInstallations;

    protected List<AirQualityMeasurement> _newMeasurements;
    protected List<AirQualityForecast> _newForecasts;

    protected ForecastErrorClass _forecastErrorClass;

    protected ForecastErrorsCalculator(IConfiguration config)
    {
      _idForAllInstallations = config.GetValue<short>(
          "AppSettings:AirlyApi:IdForAllInstallations");
    }

    public ForecastErrorClass ErrorClass => _forecastErrorClass;

    public virtual List<AirQualityForecastError> CalculateHourly(
        short installationId,
        List<AirQualityMeasurement> newMeasurements,
        List<AirQualityForecast> newForecasts)
    {
      int i = 0, j = 0;
      _newMeasurements = newMeasurements;
      _newForecasts = newForecasts;

      var calculatedForecastErrors = new List<AirQualityForecastError>();

      for (; i < _newMeasurements.Count && j < _newForecasts.Count;)
      {
        var currentMeasurementDateTime = _newMeasurements[i].TillDateTime;
        var currentForecastDateTime = _newForecasts[j].TillDateTime;

        if (currentForecastDateTime == currentMeasurementDateTime)
        {
          var forecastHourlyError =
              CalculateHourlyForecastError(installationId, i, j);

          calculatedForecastErrors.Add(forecastHourlyError);
          i++; j++;
        }
        else if (currentForecastDateTime > currentMeasurementDateTime)
        {
          _newMeasurements.RemoveAt(i);
        }
        else
        {
          _newForecasts.RemoveAt(j);
        }
      }

      return calculatedForecastErrors;
    }

    protected abstract AirQualityForecastError CalculateHourlyForecastError(
        short installationId, int i, int j);

    public virtual List<AirQualityForecastError> CalculateDaily(
        short installationId,
        short minNumberOfMeasurements,
        List<AirQualityForecastError> newHourlyForecastErrors)
    {
      var dailyForecastErrorsSum = new ErrorSum(installationId);
      var dailyForecastErrors = new List<AirQualityForecastError>();

      for (int i = 0; i < newHourlyForecastErrors.Count; i++)
      {
        var currentMeasurementRequestTime =
            newHourlyForecastErrors[i].RequestDateTime;

        var previousMeasurementRequestTime = i != 0 ?
            newHourlyForecastErrors[i - 1].RequestDateTime : DateTime.MinValue;

        // Calculate MAPE of daily forecast
        if (currentMeasurementRequestTime != previousMeasurementRequestTime)
        {
          if (i > 0 && dailyForecastErrorsSum.Counter >= minNumberOfMeasurements)
          {
            var dailyError = dailyForecastErrorsSum.CalculateMeanForecastError(
                ForecastErrorPeriod.Day, _forecastErrorClass);

            dailyForecastErrors.Add(dailyError);
          }

          dailyForecastErrorsSum.Reset(
              newHourlyForecastErrors[i].InstallationAddress,
              newHourlyForecastErrors[i].FromDateTime,
              newHourlyForecastErrors[i].RequestDateTime);
        }

        dailyForecastErrorsSum.AddAbs(newHourlyForecastErrors[i]);
      }

      if (dailyForecastErrorsSum.Counter >= minNumberOfMeasurements)
      {
        var lastDailyError = dailyForecastErrorsSum.CalculateMeanForecastError(
            ForecastErrorPeriod.Day, _forecastErrorClass);

        dailyForecastErrors.Add(lastDailyError);
      }

      return dailyForecastErrors;
    }

    public virtual AirQualityForecastError CalculateTotal(
        short installationId,
        IEnumerable<AirQualityForecastError> allForecastErrors)
    {
      var errorSum = new ErrorSum(installationId)
      {
        InstallationAddress = allForecastErrors.Last().InstallationAddress,
        FromDateTime = allForecastErrors.Min(x => x.FromDateTime),
        TillDateTime = allForecastErrors.Max(x => x.TillDateTime),
        CaqiPct = allForecastErrors.Sum(fe => fe.AirlyCaqiPct),
        Pm25Pct = allForecastErrors.Sum(fe => fe.Pm25Pct),
        Pm10Pct = allForecastErrors.Sum(fe => fe.Pm10Pct),
        Caqi = allForecastErrors.Sum(fe => fe.AirlyCaqi),
        Pm25 = allForecastErrors.Sum(fe => fe.Pm25),
        Pm10 = allForecastErrors.Sum(fe => fe.Pm10),
        RequestDateTime = allForecastErrors.Max(x => x.RequestDateTime),
        Counter = allForecastErrors.Count(),
      };

      if (installationId == _idForAllInstallations)
      {
        errorSum.InstallationAddress = "";
      }

      return errorSum.CalculateMeanForecastError(
          ForecastErrorPeriod.Total, _forecastErrorClass);
    }
  }
}
