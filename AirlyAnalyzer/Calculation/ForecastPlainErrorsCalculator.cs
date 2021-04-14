namespace AirlyAnalyzer.Calculation
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Models;

  public class ForecastPlainErrorsCalculator : IForecastErrorsCalculator
  {
    private List<AirQualityMeasurement> _newMeasurements;
    private List<AirQualityForecast> _newForecasts;

    private readonly ForecastErrorClass _forecastErrorClass;

    public ForecastPlainErrorsCalculator()
    {
      _forecastErrorClass = ForecastErrorClass.Plain;
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

    public List<AirQualityForecastError> CalculateDaily(
        short installationId,
        short minNumberOfMeasurements,
        List<AirQualityForecastError> newHourlyForecastErrors)
    {
      string installationAddress = newHourlyForecastErrors.Count > 0 ?
          newHourlyForecastErrors.Last().InstallationAddress : "";

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
                ForecastErrorPeriod.Day, ForecastErrorClass.Plain);

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
          ForecastErrorPeriod.Day, ForecastErrorClass.Plain);

        dailyForecastErrors.Add(lastDailyError);
      }

      return dailyForecastErrors;
    }

    private AirQualityForecastError CalculateHourlyForecastError(
        short installationId, int i, int j)
    {
      short pm25Error = (short)
          (_newMeasurements[i].Pm25 - _newForecasts[j].Pm25);

      short pm10Error = (short)
          (_newMeasurements[i].Pm10 - _newForecasts[j].Pm10);

      short airlyCaqiError = (short)
          (_newMeasurements[i].AirlyCaqi - _newForecasts[j].AirlyCaqi);

      double pm25Measurement = _newMeasurements[i].Pm25 > 0 ?
          (double)_newMeasurements[i].Pm25 : 1;

      double pm10Measurement = _newMeasurements[i].Pm10 > 0 ?
          (double)_newMeasurements[i].Pm10 : 1;

      double airlyCaqiMeasurement = _newMeasurements[i].AirlyCaqi > 0 ?
          (double)_newMeasurements[i].AirlyCaqi : 1;

      double pm25RelativeError = (double)pm25Error / pm25Measurement;
      double pm10RelativeError = (double)pm10Error / pm10Measurement;
      double airlyCaqiRelativeError = (double)airlyCaqiError / airlyCaqiMeasurement;

      return new AirQualityForecastError
      {
        InstallationId = installationId,
        InstallationAddress = _newMeasurements[i].InstallationAddress,
        FromDateTime = _newMeasurements[i].FromDateTime,
        TillDateTime = _newMeasurements[i].TillDateTime,
        AirlyCaqiPct = Convert.ToInt16(airlyCaqiRelativeError * 100),
        Pm25Pct = Convert.ToInt16(pm25RelativeError * 100),
        Pm10Pct = Convert.ToInt16(pm10RelativeError * 100),
        AirlyCaqi = airlyCaqiError,
        Pm25 = pm25Error,
        Pm10 = pm10Error,
        RequestDateTime = _newMeasurements[i].RequestDateTime,
        Period = ForecastErrorPeriod.Hour,
        Class = ForecastErrorClass.Plain,
      };
    }

    public AirQualityForecastError CalculateTotal(
        short installationId, IEnumerable<AirQualityForecastError> allForecastErrors)
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
          ForecastErrorPeriod.Total, ForecastErrorClass.Plain);
    }
  }
}
