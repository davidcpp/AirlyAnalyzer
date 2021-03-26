namespace AirlyAnalyzer.Client
{
  using System.Net;
  using System.Threading.Tasks;

  public class WebClientAdapter : IWebClientAdapter
  {
    private readonly WebClient _webClient;

    public WebClientAdapter(WebClient webClient)
    {
      _webClient = webClient;
    }

    public string BaseAddress
    {
      get => _webClient.BaseAddress;
      set => _webClient.BaseAddress = value;
    }

    public WebHeaderCollection Headers
    {
      get => _webClient.Headers;
      set => _webClient.Headers = value;
    }

    public Task<string> DownloadStringTaskAsync(string address)
    {
      return _webClient.DownloadStringTaskAsync(address);
    }
  }
}
