using AirlyAnalyzer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirlyAnalyzer.Models
{
  public class ForecastErrorsCalculation
  {
    private List<AirQualityMeasurement> _archiveMeasurements;
    private List<AirQualityForecast> _archiveForecasts;
    private List<AirQualityForecastError> _archiveForecastErrors;
    private List<AirQualityMeasurement> _newArchiveMeasurements;
    private List<AirQualityForecast> _newArchiveForecasts;

    private readonly AirlyContext _context;
    private readonly List<short> _installationIdsList;

    /// <summary>
    /// Minimal number of measurements to calculate daily forecast error
    /// </summary>
    private readonly short _minNumberOfMeasurements;

    public ForecastErrorsCalculation(
      AirlyContext context, List<short> installationIdsList, short minNumberOfMeasurements)
    {
      _context = context;
      _installationIdsList = installationIdsList;
      _minNumberOfMeasurements = minNumberOfMeasurements;
    }

    public List<AirQualityForecastError> CalculatedForecastErrors { get; }
      = new List<AirQualityForecastError>();

    public void CalculateAllNewForecastErrors()
    {
      AirQualityForecastError dailyForecastError;
      var dailyForecastErrorsSum = new ErrorSum();

      for (int index = 0; index < _installationIdsList.Count; index++)
      {
        short installationId = _installationIdsList[index];
        int i = 0, j = 0;

        SelectArchiveDataForId(installationId);
        SelectNotProcessedArchiveData();

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

        if (dailyForecastErrorsSum.Counter > 23)
        {
          dailyForecastErrorsSum.LastForecastIndex = i - 1;
          var lastDailyError = CalculateMeanForecastError(
            dailyForecastErrorsSum, ForecastErrorType.Daily, installationId);
          CalculatedForecastErrors.Add(lastDailyError);
        }

        dailyForecastErrorsSum.Reset(0);
      }
    }

    public async Task<int> SaveResultsInDatabase()
    {
      _context.ForecastErrors.AddRange(CalculatedForecastErrors);
      return await _context.SaveChangesAsync();
    }

    private AirQualityForecastError CalculateHourlyForecastError(
      short installationId, int i, int j)
    {
      double pm25RelativeError =
        (double)(_newArchiveMeasurements[i].Pm25 - _newArchiveForecasts[j].Pm25)
        / (double)_newArchiveMeasurements[i].Pm25;

      double pm10RelativeError =
        (double)(_newArchiveMeasurements[i].Pm10 - _newArchiveForecasts[j].Pm10)
        / (double)_newArchiveMeasurements[i].Pm10;

      double airlyCaqiRelativeError =
        (double)(_newArchiveMeasurements[i].AirlyCaqi - _newArchiveForecasts[j].AirlyCaqi)
        / (double)_newArchiveMeasurements[i].AirlyCaqi;

      var forecastError = new AirQualityForecastError
      {
        InstallationId = installationId,
        FromDateTime = _newArchiveMeasurements[i].FromDateTime,
        TillDateTime = _newArchiveMeasurements[i].TillDateTime,
        AirlyCaqiPctError = Convert.ToInt16(airlyCaqiRelativeError * 100),
        Pm25PctError = Convert.ToInt16(pm25RelativeError * 100),
        Pm10PctError = Convert.ToInt16(pm10RelativeError * 100),
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
        AirlyCaqiPctError = (short)(errorSum.Caqi / errorSum.Counter),
        Pm25PctError = (short)(errorSum.Pm25 / errorSum.Counter),
        Pm10PctError = (short)(errorSum.Pm10 / errorSum.Counter),
        RequestDateTime = _newArchiveMeasurements[errorSum.LastForecastIndex].RequestDateTime,
        ErrorType = errorType,
      };
    }

    private void SelectArchiveDataForId(short installationId)
    {
      _archiveMeasurements = _context.ArchiveMeasurements
        .Where(x => x.InstallationId == installationId).ToList();

      _archiveForecasts = _context.ArchiveForecasts
        .Where(x => x.InstallationId == installationId).ToList();

      _archiveForecastErrors = _context.ForecastErrors
        .Where(x => x.InstallationId == installationId).ToList();
    }

    private void SelectNotProcessedArchiveData()
    {
      int measurementsStartIndex;
      int forecastsStartIndex;
      int numberOfElements;

      var lastForecastError = _archiveForecastErrors.Count > 0 ?
        _archiveForecastErrors.Last() : new AirQualityForecastError();

      int i = _archiveMeasurements.Count - 1;
      while (i >= 0 && lastForecastError.TillDateTime < _archiveMeasurements[i].TillDateTime)
      {
        i--;
      }
      measurementsStartIndex = ++i;

      i = _archiveForecasts.Count - 1;
      while (i >= 0 && lastForecastError.TillDateTime < _archiveForecasts[i].TillDateTime)
      {
        i--;
      }
      forecastsStartIndex = ++i;

      numberOfElements = _archiveMeasurements.Count - measurementsStartIndex;

      // Measurements and forecasts for which there are no calculated forecast errors
      _newArchiveMeasurements = _archiveMeasurements.GetRange(measurementsStartIndex, numberOfElements);
      _newArchiveForecasts = _archiveForecasts.GetRange(forecastsStartIndex, numberOfElements);
    }

    private class ErrorSum
    {
      public int Caqi { get; set; } = 0;
      public int Pm25 { get; set; } = 0;
      public int Pm10 { get; set; } = 0;

      public int Counter { get; set; } = 0;
      public int FirstForecastIndex { get; set; } = 0;
      public int LastForecastIndex { get; set; } = 0;

      public ErrorSum()
      {
      }

      public ErrorSum(int caqi, int pm25, int pm10, int counter)
      {
        Caqi = caqi;
        Pm25 = pm25;
        Pm10 = pm10;
        Counter = counter;
      }

      public void Add(ErrorSum errorSum)
      {
        Caqi += errorSum?.Caqi ?? 0;
        Pm25 += errorSum?.Pm25 ?? 0;
        Pm10 += errorSum?.Pm10 ?? 0;
      }

      public void AddAbs(AirQualityForecastError error)
      {
        Caqi += Math.Abs(error.AirlyCaqiPctError);
        Pm25 += Math.Abs(error.Pm25PctError);
        Pm10 += Math.Abs(error.Pm10PctError);
        Counter++;
      }

      public void Reset(int firstForecastIndex)
      {
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
