namespace AirlyAnalyzer.Calculation
{
  using System;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;

  public sealed class ScaleForecastErrorsCalculator : ForecastErrorsCalculator
  {
    private readonly short _caqiScaleLevel;

    public ScaleForecastErrorsCalculator(IConfiguration config) : base(config)
    {
      _caqiScaleLevel = config.GetValue<short>(
          "AppSettings:AirlyApi:CaqiScaleLevel");

      _forecastErrorClass = ForecastErrorClass.Scale;
    }

    protected override AirQualityForecastError CalculateHourlyForecastError(
        short installationId, int i, int j, AirQualityDataSource forecastSource)
    {
      int airlyCaqiError = 0;

      if (_newMeasurements[i].AirlyCaqi > 2 * _caqiScaleLevel
          || _newForecasts[j].AirlyCaqi > 2 * _caqiScaleLevel)
      {
        airlyCaqiError = (_newMeasurements[i].AirlyCaqi / _caqiScaleLevel)
            - (_newForecasts[j].AirlyCaqi / _caqiScaleLevel);
      }

      return new AirQualityForecastError
      {
        InstallationId = installationId,
        InstallationAddress = _newMeasurements[i].InstallationAddress,
        FromDateTime = _newMeasurements[i].FromDateTime,
        TillDateTime = _newMeasurements[i].TillDateTime,
        AirlyCaqiPct = (short)(Math.Abs(airlyCaqiError) >= 1 ? 100 : 0),
        AirlyCaqi = (short)airlyCaqiError,
        RequestDateTime = _newMeasurements[i].RequestDateTime,
        Period = ForecastErrorPeriod.Hour,
        Class = ForecastErrorClass.Scale,
        Source = forecastSource,
      };
    }
  }
}
