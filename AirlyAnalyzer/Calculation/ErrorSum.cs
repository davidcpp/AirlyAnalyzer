namespace AirlyAnalyzer.Calculation
{
  using AirlyAnalyzer.Models;
  using System;

  public class ErrorSum
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
