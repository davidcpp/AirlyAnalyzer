namespace AirlyAnalyzer.Models
{
  using Microsoft.Extensions.Configuration;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class ForecastErrorsCalculation
  {
    private List<AirQualityMeasurement> _newArchiveMeasurements;
    private List<AirQualityForecast> _newArchiveForecasts;
    private readonly List<AirQualityForecastError> _calculatedForecastErrors
      = new List<AirQualityForecastError>();
    private readonly ErrorSum _dailyForecastErrorsSum = new ErrorSum();

    private readonly DatabaseHelper _databaseHelper;

    private readonly List<short> _installationIdsList;
    private readonly short _idForAllInstallations;

    /// <summary>
    /// Minimal number of measurements to calculate daily forecast error
    /// </summary>
    private readonly short _minNumberOfMeasurements;

    public ForecastErrorsCalculation(
      DatabaseHelper databaseHelper,
      IConfiguration config,
      List<short> installationIdsList,
      short minNumberOfMeasurements)
    {
      _databaseHelper = databaseHelper;

      _installationIdsList = installationIdsList;
      _minNumberOfMeasurements = minNumberOfMeasurements;

      _idForAllInstallations = config.GetValue<sbyte>("AppSettings:AirlyApi:IdForAllInstallations");
    }

    public List<AirQualityForecastError> CalculateAllNewForecastErrors()
    {
      foreach (short installationId in _installationIdsList)
      {
        int i = 0, j = 0;

        _databaseHelper.SelectDataToProcessing(
          installationId, out _newArchiveMeasurements, out _newArchiveForecasts);

        for (; i < _newArchiveMeasurements.Count && j < _newArchiveForecasts.Count;)
        {
          var currentMeasurementDateTime = _newArchiveMeasurements[i].TillDateTime;
          var currentForecastDateTime = _newArchiveForecasts[j].TillDateTime;

          if (currentForecastDateTime == currentMeasurementDateTime)
          {
            CalculateNextForecastErrors(installationId, i, j);
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

        if (_dailyForecastErrorsSum.Counter >= _minNumberOfMeasurements)
        {
          var lastDailyError = _dailyForecastErrorsSum.CalculateMeanForecastError(ForecastErrorType.Daily);
          _calculatedForecastErrors.Add(lastDailyError);
        }
      }

      return _calculatedForecastErrors;
    }

    private AirQualityForecastError CalculateHourlyForecastError(
      short installationId, int i, int j)
    {
      short pm25Error = (short)(_newArchiveMeasurements[i].Pm25 - _newArchiveForecasts[j].Pm25);
      short pm10Error = (short)(_newArchiveMeasurements[i].Pm10 - _newArchiveForecasts[j].Pm10);
      short airlyCaqiError = (short)(_newArchiveMeasurements[i].AirlyCaqi - _newArchiveForecasts[j].AirlyCaqi);

      double pm25Measurement =
        _newArchiveMeasurements[i].Pm25 > 0 ? (double)_newArchiveMeasurements[i].Pm25 : 1;

      double pm10Measurement =
        _newArchiveMeasurements[i].Pm10 > 0 ? (double)_newArchiveMeasurements[i].Pm10 : 1;

      double airlyCaqiMeasurement =
        _newArchiveMeasurements[i].AirlyCaqi > 0 ? (double)_newArchiveMeasurements[i].AirlyCaqi : 1;

      double pm25RelativeError = (double)pm25Error / pm25Measurement;
      double pm10RelativeError = (double)pm10Error / pm10Measurement;
      double airlyCaqiRelativeError = (double)airlyCaqiError / airlyCaqiMeasurement;

      return new AirQualityForecastError
      {
        InstallationId = installationId,
        FromDateTime = _newArchiveMeasurements[i].FromDateTime,
        TillDateTime = _newArchiveMeasurements[i].TillDateTime,
        AirlyCaqiPctError = Convert.ToInt16(airlyCaqiRelativeError * 100),
        Pm25PctError = Convert.ToInt16(pm25RelativeError * 100),
        Pm10PctError = Convert.ToInt16(pm10RelativeError * 100),
        AirlyCaqiError = airlyCaqiError,
        Pm25Error = pm25Error,
        Pm10Error = pm10Error,
        RequestDateTime = _newArchiveMeasurements[i].RequestDateTime,
        ErrorType = ForecastErrorType.Hourly,
      };
    }

    private void CalculateNextForecastErrors(
      short installationId, int i, int j)
    {
      var currentMeasurementRequestTime = _newArchiveMeasurements[i].RequestDateTime;
      var previousMeasurementRequestTime = i != 0 ?
        _newArchiveMeasurements[i - 1].RequestDateTime : DateTime.MinValue;

      // Calculate MAPE of daily forecast
      if (currentMeasurementRequestTime != previousMeasurementRequestTime)
      {
        if (i > 0 && _dailyForecastErrorsSum.Counter >= _minNumberOfMeasurements)
        {
          var dailyForecastError = _dailyForecastErrorsSum.CalculateMeanForecastError(ForecastErrorType.Daily);
          _calculatedForecastErrors.Add(dailyForecastError);
        }

        _dailyForecastErrorsSum.Reset(
          installationId, _newArchiveMeasurements[i].FromDateTime, _newArchiveMeasurements[i].RequestDateTime);
      }

      var forecastHourlyError = CalculateHourlyForecastError(installationId, i, j);
      _calculatedForecastErrors.Add(forecastHourlyError);

      _dailyForecastErrorsSum.AddAbs(forecastHourlyError);
    }

    public List<AirQualityForecastError> CalculateAllTotalForecastErrors()
    {
      var calculatedForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIdsList)
      {
        var dailyForecastErrors = _databaseHelper.SelectDailyForecastErrors(installationId);

        if (dailyForecastErrors.Count > 0)
        {
          var installationForecastError = CalculateTotalForecastError(dailyForecastErrors, installationId);
          calculatedForecastErrors.Add(installationForecastError);
        }
      }

      if (calculatedForecastErrors.Count > 0)
      {
        // Remove old total forecast errors before adding updated ones
        _databaseHelper.RemoveTotalForecastErrors();

        // Assumption of the latest TillDateTime in last totalForecastErrors element
        calculatedForecastErrors.OrderBy(e => e.FromDateTime);
        var totalForecastError = CalculateTotalForecastError(calculatedForecastErrors, _idForAllInstallations);
        calculatedForecastErrors.Add(totalForecastError);
      }

      return calculatedForecastErrors;
    }

    private AirQualityForecastError CalculateTotalForecastError(
      List<AirQualityForecastError> allForecastErrors, short installationId)
    {
      var errorSum = new ErrorSum
      {
        InstallationId = installationId,
        FromDateTime = allForecastErrors[0].FromDateTime,
        TillDateTime = allForecastErrors.Last().TillDateTime,
        CaqiPct = allForecastErrors.Sum(e => e.AirlyCaqiPctError),
        Pm25Pct = allForecastErrors.Sum(e => e.Pm25PctError),
        Pm10Pct = allForecastErrors.Sum(e => e.Pm10PctError),
        Caqi = allForecastErrors.Sum(e => e.AirlyCaqiError),
        Pm25 = allForecastErrors.Sum(e => e.Pm25Error),
        Pm10 = allForecastErrors.Sum(e => e.Pm10Error),
        RequestDateTime = allForecastErrors.Last().RequestDateTime,
        Counter = allForecastErrors.Count,
      };

      return errorSum.CalculateMeanForecastError(ForecastErrorType.Total);
    }

    private class ErrorSum
    {
      private DateTime _fromDateTime;
      private DateTime _tillDateTime;
      private DateTime _requestDateTime;

      public int CaqiPct { get; set; } = 0;
      public int Pm25Pct { get; set; } = 0;
      public int Pm10Pct { get; set; } = 0;

      public int Caqi { get; set; } = 0;
      public int Pm25 { get; set; } = 0;
      public int Pm10 { get; set; } = 0;

      public short InstallationId { get; set; } = 0;
      public int Counter { get; set; } = 0;

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
        CaqiPct += Math.Abs(error.AirlyCaqiPctError);
        Pm25Pct += Math.Abs(error.Pm25PctError);
        Pm10Pct += Math.Abs(error.Pm10PctError);

        Caqi += Math.Abs(error.AirlyCaqiError);
        Pm25 += Math.Abs(error.Pm25Error);
        Pm10 += Math.Abs(error.Pm10Error);

        TillDateTime = error.TillDateTime;
        Counter++;
      }

      public void Reset(short installationId, DateTime fromDateTime, DateTime requestDateTime)
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

      public AirQualityForecastError CalculateMeanForecastError(ForecastErrorType errorType)
      {
        return new AirQualityForecastError
        {
          InstallationId = this.InstallationId,
          FromDateTime = this.FromDateTime,
          TillDateTime = this.TillDateTime,
          AirlyCaqiPctError = (short)(CaqiPct / Counter),
          Pm25PctError = (short)(Pm25Pct / Counter),
          Pm10PctError = (short)(Pm10Pct / Counter),
          AirlyCaqiError = (short)(Caqi / Counter),
          Pm25Error = (short)(Pm25 / Counter),
          Pm10Error = (short)(Pm10 / Counter),
          RequestDateTime = this.RequestDateTime,
          ErrorType = errorType,
        };
      }
    }
  }
}
