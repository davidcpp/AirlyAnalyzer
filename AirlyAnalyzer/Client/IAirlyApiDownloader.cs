namespace AirlyAnalyzer.Client
{
  using AirlyAnalyzer.Models;
  using System;
  using System.Threading.Tasks;

  public interface IAirlyApiDownloader : IDisposable
  {
    public Task<Installation> DownloadInstallationInfo(short installationId);
    public Task<Measurements> DownloadInstallationMeasurements(short installationId);
  }
}
