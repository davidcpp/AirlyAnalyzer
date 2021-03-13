namespace AirlyAnalyzer.Client
{
  using System.Threading.Tasks;

  public interface IAirQualityDataDownloader<T>
  {
    public Task<T> DownloadAirQualityData(short installationId);
  }
}
