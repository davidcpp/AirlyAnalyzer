namespace AirlyAnalyzer.Client
{
  using System;
  using System.Threading.Tasks;

  public interface IAirQualityDataDownloader<T> : IDisposable
  {
    Task<T> DownloadAirQualityData(short installationId);
  }
}
