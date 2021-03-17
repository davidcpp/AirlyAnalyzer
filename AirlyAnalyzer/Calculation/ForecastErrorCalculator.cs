namespace AirlyAnalyzer.Calculation
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AirlyAnalyzer.Models;

  public class ForecastErrorsCalculator
  {
    private List<AirQualityMeasurement> _newArchiveMeasurements;
    private List<AirQualityForecast> _newArchiveForecasts;

    /// <summary>
    /// Minimal number of measurements to calculate daily forecast error
    /// </summary>
    private readonly short _minNumberOfMeasurements;

    public ForecastErrorsCalculator(short minNumberOfMeasurements)
    {
      _minNumberOfMeasurements = minNumberOfMeasurements;
    }

    public List<AirQualityForecastError> CalculateHourlyForecastErrors(
        short installationId,
        List<AirQualityMeasurement> newArchiveMeasurements,
        List<AirQualityForecast> newArchiveForecasts)
    {
      int i = 0, j = 0;
      _newArchiveMeasurements = newArchiveMeasurements;
      _newArchiveForecasts = newArchiveForecasts;

      var calculatedForecastErrors = new List<AirQualityForecastError>();

      for (; i < _newArchiveMeasurements.Count && j < _newArchiveForecasts.Count;)
      {
        var currentMeasurementDateTime = _newArchiveMeasurements[i].TillDateTime;
        var currentForecastDateTime = _newArchiveForecasts[j].TillDateTime;

        if (currentForecastDateTime == currentMeasurementDateTime)
        {
          var forecastHourlyError =
              CalculateHourlyForecastError(installationId, i, j);

          calculatedForecastErrors.Add(forecastHourlyError);
          i++; j++;
        }
        else if (currentForecastDateTime > currentMeasurementDateTime)
        {
          _newArchiveMeasurements.RemoveAt(i);
        }
        else
        {
          _newArchiveForecasts.RemoveAt(j);
        }
      }

      return calculatedForecastErrors;
    }

    public List<AirQualityForecastError> CalculateDailyForecastErrors(
        short installationId,
        List<AirQualityForecastError> newHourlyForecastErrors)
    {
      var dailyForecastErrorsSum = new ErrorSum();
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
          if (i > 0 && dailyForecastErrorsSum.Counter >= _minNumberOfMeasurements)
          {
            var dailyError = dailyForecastErrorsSum
                .CalculateMeanForecastError(ForecastErrorType.Daily);

            dailyForecastErrors.Add(dailyError);
          }

          dailyForecastErrorsSum.Reset(
              installationId,
              newHourlyForecastErrors[i].FromDateTime,
              newHourlyForecastErrors[i].RequestDateTime);
        }

        dailyForecastErrorsSum.AddAbs(newHourlyForecastErrors[i]);
      }

      if (dailyForecastErrorsSum.Counter >= _minNumberOfMeasurements)
      {
        var lastDailyError = dailyForecastErrorsSum
            .CalculateMeanForecastError(ForecastErrorType.Daily);

        dailyForecastErrors.Add(lastDailyError);
      }

      return dailyForecastErrors;
    }

    private AirQualityForecastError CalculateHourlyForecastError(
        short installationId, int i, int j)
    {
      short pm25Error = (short)
          (_newArchiveMeasurements[i].Pm25 - _newArchiveForecasts[j].Pm25);

      short pm10Error = (short)
          (_newArchiveMeasurements[i].Pm10 - _newArchiveForecasts[j].Pm10);

      short airlyCaqiError = (short)
          (_newArchiveMeasurements[i].AirlyCaqi - _newArchiveForecasts[j].AirlyCaqi);

      double pm25Measurement = _newArchiveMeasurements[i].Pm25 > 0 ?
          (double)_newArchiveMeasurements[i].Pm25 : 1;

      double pm10Measurement = _newArchiveMeasurements[i].Pm10 > 0 ?
          (double)_newArchiveMeasurements[i].Pm10 : 1;

      double airlyCaqiMeasurement = _newArchiveMeasurements[i].AirlyCaqi > 0 ?
          (double)_newArchiveMeasurements[i].AirlyCaqi : 1;

      double pm25RelativeError = (double)pm25Error / pm25Measurement;
      double pm10RelativeError = (double)pm10Error / pm10Measurement;
      double airlyCaqiRelativeError = (double)airlyCaqiError / airlyCaqiMeasurement;

      return new AirQualityForecastError
      {
        InstallationId = installationId,
        FromDateTime = _newArchiveMeasurements[i].FromDateTime,
        TillDateTime = _newArchiveMeasurements[i].TillDateTime,
        AirlyCaqiPct = Convert.ToInt16(airlyCaqiRelativeError * 100),
        Pm25Pct = Convert.ToInt16(pm25RelativeError * 100),
        Pm10Pct = Convert.ToInt16(pm10RelativeError * 100),
        AirlyCaqi = airlyCaqiError,
        Pm25 = pm25Error,
        Pm10 = pm10Error,
        RequestDateTime = _newArchiveMeasurements[i].RequestDateTime,
        ErrorType = ForecastErrorType.Hourly,
      };
    }

    public AirQualityForecastError CalculateTotalForecastError(
        short installationId, IEnumerable<AirQualityForecastError> allForecastErrors)
    {
      var errorSum = new ErrorSum
      {
        InstallationId = installationId,
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

      return errorSum.CalculateMeanForecastError(ForecastErrorType.Total);
    }

    private class ErrorSum
    {
      private DateTime _fromDateTime;
      private DateTime _tillDateTime;
      private DateTime _requestDateTime;

      public short InstallationId { get; set; } = 0;
      public int Counter { get; set; } = 0;

      public int CaqiPct { get; set; } = 0;
      public int Pm25Pct { get; set; } = 0;
      public int Pm10Pct { get; set; } = 0;
      public int Caqi { get; set; } = 0;
      public int Pm25 { get; set; } = 0;
      public int Pm10 { get; set; } = 0;

      public DateTime FromDateTime
      {
        get => _fromDateTime.ToLocalTime();
        set => _fromDateTime = value.ToUniversalTime();
      }

      public DateTime TillDateTime
      {
        get => _tillDateTime.ToLocalTime();
        set => _tillDateTime = value.ToUniversalTime();
      }

      public DateTime RequestDateTime
      {
        get => _requestDateTime.ToLocalTime();
        set => _requestDateTime = value.ToUniversalTime();
      }

      public void AddAbs(AirQualityForecastError error)
      {
        CaqiPct += Math.Abs(error.AirlyCaqiPct);
        Pm25Pct += Math.Abs(error.Pm25Pct);
        Pm10Pct += Math.Abs(error.Pm10Pct);
        Caqi += Math.Abs(error.AirlyCaqi);
        Pm25 += Math.Abs(error.Pm25);
        Pm10 += Math.Abs(error.Pm10);

        TillDateTime = error.TillDateTime;
        Counter++;
      }

      public void Reset(
          short installationId, DateTime fromDateTime, DateTime requestDateTime)
      {
        InstallationId = installationId;

        FromDateTime = fromDateTime;
        TillDateTime = DateTime.MinValue;
        RequestDateTime = requestDateTime;

        CaqiPct = 0;
        Pm25Pct = 0;
        Pm10Pct = 0;
        Caqi = 0;
        Pm25 = 0;
        Pm10 = 0;

        Counter = 0;
      }

      public AirQualityForecastError CalculateMeanForecastError(
          ForecastErrorType errorType)
      {
        return new AirQualityForecastError
        {
          InstallationId = this.InstallationId,
          FromDateTime = this.FromDateTime,
          TillDateTime = this.TillDateTime,
          AirlyCaqiPct = (short)(CaqiPct / Counter),
          Pm25Pct = (short)(Pm25Pct / Counter),
          Pm10Pct = (short)(Pm10Pct / Counter),
          AirlyCaqi = (short)(Caqi / Counter),
          Pm25 = (short)(Pm25 / Counter),
          Pm10 = (short)(Pm10 / Counter),
          RequestDateTime = this.RequestDateTime,
          ErrorType = errorType,
        };
      }
    }
  }
}
