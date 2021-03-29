namespace AirlyAnalyzer.Client
{
  using System;
  using System.Net;
  using System.Threading.Tasks;

  public interface IWebClientAdapter : IDisposable
  {
    string BaseAddress { get; set; }

    WebHeaderCollection Headers { get; set; }

    Task<string> DownloadStringTaskAsync(string address);
  }
}
