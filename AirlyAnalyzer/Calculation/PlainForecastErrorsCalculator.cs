namespace AirlyAnalyzer.Calculation
{
  using System;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;

  public sealed class PlainForecastErrorsCalculator : ForecastErrorsCalculator
  {
    public PlainForecastErrorsCalculator(IConfiguration config) : base(config)
    {
      _forecastErrorClass = ForecastErrorClass.Plain;
    }

    protected override AirQualityForecastError CalculateHourlyForecastError(
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
  }
}
