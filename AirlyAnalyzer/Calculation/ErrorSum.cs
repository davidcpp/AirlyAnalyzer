﻿namespace AirlyAnalyzer.Calculation
{
  using AirlyAnalyzer.Models;
  using System;

  public class ErrorSum
  {
    private DateTime _fromDateTime;
    private DateTime _tillDateTime;
    private DateTime _requestDateTime;

    public ErrorSum()
    {
    }

    public ErrorSum(short installationId)
    {
      InstallationId = installationId;
    }

    public short InstallationId { get; } = 0;

    public string InstallationAddress { get; set; }

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

    public void Reset(string installationAddress, DateTime fromDateTime, DateTime requestDateTime)
    {
      InstallationAddress = installationAddress;

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
        ForecastErrorPeriod errorType, ForecastErrorClass errorClass)
    {
      return new AirQualityForecastError
      {
        InstallationId = this.InstallationId,
        InstallationAddress = this.InstallationAddress,
        FromDateTime = this.FromDateTime,
        TillDateTime = this.TillDateTime,
        AirlyCaqiPct = (short)(CaqiPct / Counter),
        Pm25Pct = (short)(Pm25Pct / Counter),
        Pm10Pct = (short)(Pm10Pct / Counter),
        AirlyCaqi = (short)(Caqi / Counter),
        Pm25 = (short)(Pm25 / Counter),
        Pm10 = (short)(Pm10 / Counter),
        RequestDateTime = this.RequestDateTime,
        Period = errorType,
        Class = errorClass,
      };
    }
  }
}
