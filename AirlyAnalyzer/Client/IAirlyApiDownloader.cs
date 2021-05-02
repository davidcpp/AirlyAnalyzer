namespace AirlyAnalyzer.Client
{
  using System;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;

  public interface IAirlyApiDownloader : IDisposable
  {
    public Task<Installation> DownloadInstallationInfo(short installationId);
    public Task<Measurements> DownloadInstallationMeasurements(short installationId);
  }
}
