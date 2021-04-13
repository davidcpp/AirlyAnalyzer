namespace AirlyAnalyzer.Calculation
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;

  public class ForecastScaleErrorsCalculator : IForecastErrorsCalculator
  {
    private readonly short _caqiScaleLevel;

    private List<AirQualityMeasurement> _newMeasurements;
    private List<AirQualityForecast> _newForecasts;

    private readonly ForecastErrorClass _forecastErrorClass;

    public ForecastScaleErrorsCalculator(IConfiguration config)
    {
      _caqiScaleLevel = config.GetValue<short>(
          "AppSettings:AirlyApi:CaqiScaleLevel");

      _forecastErrorClass = ForecastErrorClass.Scale;
    }

    public ForecastErrorClass ErrorClass => _forecastErrorClass;

    public List<AirQualityForecastError> CalculateHourly(
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

    private AirQualityForecastError CalculateHourlyForecastError(
        short installationId, int i, int j)
    {
      int airlyCaqiError = (_newMeasurements[i].AirlyCaqi / _caqiScaleLevel)
          - (_newForecasts[j].AirlyCaqi / _caqiScaleLevel);

      return new AirQualityForecastError
      {
        InstallationId = installationId,
        InstallationAddress = _newMeasurements[i].InstallationAddress,
        FromDateTime = _newMeasurements[i].FromDateTime,
        TillDateTime = _newMeasurements[i].TillDateTime,
        AirlyCaqiPct = (short)(airlyCaqiError >= 1 ? 100 : 0),
        AirlyCaqi = (short)airlyCaqiError,
        RequestDateTime = _newMeasurements[i].RequestDateTime,
        Period = ForecastErrorPeriod.Hour,
        Class = ForecastErrorClass.Scale,
      };
    }

    public List<AirQualityForecastError> CalculateDaily(
        short installationId,
        short minNumberOfMeasurements,
        List<AirQualityForecastError> newHourlyForecastErrors)
    {
      string installationAddress = newHourlyForecastErrors.Count > 0 ?
          newHourlyForecastErrors[0].InstallationAddress : "";

      var dailyForecastErrorsSum
          = new ErrorSum(installationId, installationAddress);

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
                ForecastErrorPeriod.Day, ForecastErrorClass.Scale);

            dailyForecastErrors.Add(dailyError);
          }

          dailyForecastErrorsSum.Reset(
              newHourlyForecastErrors[i].FromDateTime,
              newHourlyForecastErrors[i].RequestDateTime);
        }

        dailyForecastErrorsSum.AddAbs(newHourlyForecastErrors[i]);
      }

      if (dailyForecastErrorsSum.Counter >= minNumberOfMeasurements)
      {
        var lastDailyError = dailyForecastErrorsSum.CalculateMeanForecastError(
            ForecastErrorPeriod.Day, ForecastErrorClass.Scale);

        dailyForecastErrors.Add(lastDailyError);
      }

      return dailyForecastErrors;
    }

    public AirQualityForecastError CalculateTotal(
        short installationId,
        IEnumerable<AirQualityForecastError> allForecastErrors)
    {
      var errorSum = new ErrorSum
      {
        InstallationId = installationId,
        InstallationAddress = allForecastErrors.Last().InstallationAddress,
        FromDateTime = allForecastErrors.First().FromDateTime,
        TillDateTime = allForecastErrors.Last().TillDateTime,
        CaqiPct = allForecastErrors.Sum(fe => fe.AirlyCaqiPct),
        Pm25Pct = allForecastErrors.Sum(fe => fe.Pm25Pct),
        Pm10Pct = allForecastErrors.Sum(fe => fe.Pm10Pct),
        Caqi = allForecastErrors.Sum(fe => fe.AirlyCaqi),
        Pm25 = allForecastErrors.Sum(fe => fe.Pm25),
        Pm10 = allForecastErrors.Sum(fe => fe.Pm10),
        RequestDateTime = allForecastErrors.Last().RequestDateTime,
        Counter = allForecastErrors.Count(),
      };

      return errorSum.CalculateMeanForecastError(
          ForecastErrorPeriod.Total, ForecastErrorClass.Scale);
    }
  }
}
