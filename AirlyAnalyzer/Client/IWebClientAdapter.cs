namespace AirlyAnalyzer.Client
{
  using System.Net;
  using System.Threading.Tasks;

  public interface IWebClientAdapter
  {
    string BaseAddress { get; set; }

    WebHeaderCollection Headers { get; set; }

    Task<string> DownloadStringTaskAsync(string address);
  }
}
