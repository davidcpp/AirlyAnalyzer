using AirlyAnalyzer.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirlyAnalyzer.Models
{
  public class ForecastErrorsCalculation
  {
    private List<AirQualityMeasurement> _newArchiveMeasurements;
    private List<AirQualityForecast> _newArchiveForecasts;

    private readonly AirlyContext _context;
    private readonly List<short> _installationIdsList;

    /// <summary>
    /// Minimal number of measurements to calculate daily forecast error
    /// </summary>
    private readonly short _idForAllInstallations;
    private readonly short _minNumberOfMeasurements;

    public ForecastErrorsCalculation(
      AirlyContext context, IConfiguration config, List<short> installationIdsList, short minNumberOfMeasurements)
    {
      _context = context;
      _installationIdsList = installationIdsList;
      _idForAllInstallations = config.GetValue<sbyte>("AppSettings:AirlyApi:IdForAllInstallations");
      _minNumberOfMeasurements = minNumberOfMeasurements;
    }

    public List<AirQualityForecastError> CalculatedForecastErrors { get; }
      = new List<AirQualityForecastError>();

    public async Task CalculateAll()
    {
      if (CalculateAllNewForecastErrors() > 0)
      {
        await SaveResultsInDatabase();
        CalculateAllTotalForecastErrors();
        await SaveResultsInDatabase();
      }
    }

    public int CalculateAllNewForecastErrors()
    {
      AirQualityForecastError dailyForecastError;
      var dailyForecastErrorsSum = new ErrorSum();

      for (int index = 0; index < _installationIdsList.Count; index++)
      {
        short installationId = _installationIdsList[index];
        int i = 0, j = 0;

        SelectNotProcessedArchiveData(installationId);

        for (; i < _newArchiveMeasurements.Count && j < _newArchiveForecasts.Count;)
        {
          var currentMeasurementDateTime = _newArchiveMeasurements[i].TillDateTime.ToUniversalTime();
          var currentForecastDateTime = _newArchiveForecasts[j].TillDateTime.ToUniversalTime();

          if (currentForecastDateTime == currentMeasurementDateTime)
          {
            var currentMeasurementRequestTime = _newArchiveMeasurements[i].RequestDateTime;
            var previousMeasurementRequestTime = i != 0 ?
              _newArchiveMeasurements[i - 1].RequestDateTime : new DateTime();

            // Calculate MAPE of daily forecast
            if (i != 0 && currentMeasurementRequestTime != previousMeasurementRequestTime)
            {
              if (dailyForecastErrorsSum.Counter >= _minNumberOfMeasurements)
              {
                dailyForecastErrorsSum.LastForecastIndex = i - 1;
                dailyForecastError = CalculateMeanForecastError(
                  dailyForecastErrorsSum, ForecastErrorType.Daily, installationId);

                CalculatedForecastErrors.Add(dailyForecastError);
              }

              dailyForecastErrorsSum.Reset(i);
            }

            var forecastHourlyError = CalculateHourlyForecastError(installationId, i, j);
            CalculatedForecastErrors.Add(forecastHourlyError);

            dailyForecastErrorsSum.AddAbs(forecastHourlyError);

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

        if (dailyForecastErrorsSum.Counter >= _minNumberOfMeasurements)
        {
          dailyForecastErrorsSum.LastForecastIndex = i - 1;
          var lastDailyError = CalculateMeanForecastError(
            dailyForecastErrorsSum, ForecastErrorType.Daily, installationId);

          CalculatedForecastErrors.Add(lastDailyError);
        }

        dailyForecastErrorsSum.Reset(0);
      }
      return CalculatedForecastErrors.Count;
    }

    public void CalculateAllTotalForecastErrors()
    {
      foreach (short installationId in _installationIdsList)
      {
        var dailyForecastErrors = _context.ForecastErrors
          .Where(e => e.InstallationId == installationId && e.ErrorType == ForecastErrorType.Daily)
          .ToList();

        if (dailyForecastErrors.Count > 0)
        {
          var installationForecastError = CalculateTotalForecastError(dailyForecastErrors, installationId);
          CalculatedForecastErrors.Add(installationForecastError);
        }
      }

      if (CalculatedForecastErrors.Count > 0)
      {
        RemoveOldTotalErrorsFromDatabase();

        // Assumption of the latest TillDateTime in last totalForecastErrors element
        CalculatedForecastErrors.OrderBy(e => e.FromDateTime);
        var totalForecastError = CalculateTotalForecastError(CalculatedForecastErrors, _idForAllInstallations);
        CalculatedForecastErrors.Add(totalForecastError);
      }
    }

    public async Task<int> SaveResultsInDatabase()
    {
      _context.ForecastErrors.AddRange(CalculatedForecastErrors);
      CalculatedForecastErrors.Clear();
      return await _context.SaveChangesAsync();
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

      var forecastError = new AirQualityForecastError
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

      return forecastError;
    }

    private AirQualityForecastError CalculateMeanForecastError(
      ErrorSum errorSum, ForecastErrorType errorType, short installationId)
    {
      return new AirQualityForecastError
      {
        InstallationId = installationId,
        FromDateTime = _newArchiveMeasurements[errorSum.FirstForecastIndex].FromDateTime,
        TillDateTime = _newArchiveMeasurements[errorSum.LastForecastIndex].TillDateTime,
        AirlyCaqiPctError = (short)(errorSum.CaqiPct / errorSum.Counter),
        Pm25PctError = (short)(errorSum.Pm25Pct / errorSum.Counter),
        Pm10PctError = (short)(errorSum.Pm10Pct / errorSum.Counter),
        AirlyCaqiError = (short)(errorSum.Caqi / errorSum.Counter),
        Pm25Error = (short)(errorSum.Pm25 / errorSum.Counter),
        Pm10Error = (short)(errorSum.Pm10 / errorSum.Counter),
        RequestDateTime = _newArchiveMeasurements[errorSum.LastForecastIndex].RequestDateTime,
        ErrorType = errorType,
      };
    }

    private AirQualityForecastError CalculateTotalForecastError(
      List<AirQualityForecastError> allForecastErrors, short installationId)
    {
      return new AirQualityForecastError
      {
        InstallationId = installationId,
        FromDateTime = allForecastErrors[0].FromDateTime,
        TillDateTime = allForecastErrors.Last().TillDateTime,
        AirlyCaqiPctError =
          (short)(allForecastErrors.Sum(e => e.AirlyCaqiPctError) / allForecastErrors.Count),
        Pm25PctError =
          (short)(allForecastErrors.Sum(e => e.Pm25PctError) / allForecastErrors.Count),
        Pm10PctError =
          (short)(allForecastErrors.Sum(e => e.Pm10PctError) / allForecastErrors.Count),
        AirlyCaqiError =
          (short)(allForecastErrors.Sum(e => e.AirlyCaqiError) / allForecastErrors.Count),
        Pm25Error =
          (short)(allForecastErrors.Sum(e => e.Pm25Error) / allForecastErrors.Count),
        Pm10Error =
          (short)(allForecastErrors.Sum(e => e.Pm10Error) / allForecastErrors.Count),
        RequestDateTime = allForecastErrors.Last().RequestDateTime,
        ErrorType = ForecastErrorType.Total,
      };
    }

    private void RemoveOldTotalErrorsFromDatabase()
    {
      var oldTotalForecastErrors = _context.ForecastErrors
        .Where(e => e.ErrorType == ForecastErrorType.Total)
        .ToList();

      _context.ForecastErrors.RemoveRange(oldTotalForecastErrors);
    }

    private void SelectNotProcessedArchiveData(short installationId)
    {
      var lastForecastErrorDate = DateTime.MinValue;

      if (_context.ForecastErrors.Any())
      {
        lastForecastErrorDate = _context.ForecastErrors
          .Where(e => e.InstallationId == installationId)
          .OrderByDescending(e => e.TillDateTime)
          .Select(e => e.TillDateTime)
          .First();
      }

      _newArchiveMeasurements = _context.ArchiveMeasurements
        .Where(m => m.InstallationId == installationId
                 && m.TillDateTime > lastForecastErrorDate)
        .ToList();

      if (_newArchiveMeasurements.Count == 0)
      {
        _newArchiveForecasts = new List<AirQualityForecast>();
      }
      else
      {
        var lastMeasurementDate = _newArchiveMeasurements.Last().TillDateTime.ToUniversalTime();

        _newArchiveForecasts = _context.ArchiveForecasts
          .Where(f => f.InstallationId == installationId
                   && f.TillDateTime > lastForecastErrorDate
                   && f.TillDateTime <= lastMeasurementDate)
          .ToList();
      }
    }

    private class ErrorSum
    {
      public int CaqiPct { get; set; } = 0;
      public int Pm25Pct { get; set; } = 0;
      public int Pm10Pct { get; set; } = 0;

      public int Caqi { get; set; } = 0;
      public int Pm25 { get; set; } = 0;
      public int Pm10 { get; set; } = 0;

      public int Counter { get; set; } = 0;
      public int FirstForecastIndex { get; set; } = 0;
      public int LastForecastIndex { get; set; } = 0;

      public void AddAbs(AirQualityForecastError error)
      {
        CaqiPct += Math.Abs(error.AirlyCaqiPctError);
        Pm25Pct += Math.Abs(error.Pm25PctError);
        Pm10Pct += Math.Abs(error.Pm10PctError);

        Caqi += Math.Abs(error.AirlyCaqiError);
        Pm25 += Math.Abs(error.Pm25Error);
        Pm10 += Math.Abs(error.Pm10Error);

        Counter++;
      }

      public void Reset(int firstForecastIndex)
      {
        CaqiPct = 0;
        Pm25Pct = 0;
        Pm10Pct = 0;

        Caqi = 0;
        Pm25 = 0;
        Pm10 = 0;

        Counter = 0;
        FirstForecastIndex = firstForecastIndex;
        LastForecastIndex = 0;
      }
    }
  }
}
