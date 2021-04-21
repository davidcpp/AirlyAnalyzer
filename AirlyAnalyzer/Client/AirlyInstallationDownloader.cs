namespace AirlyAnalyzer.Client
{
  using System;
  using System.Diagnostics;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;

  public class AirlyInstallationDownloader : AirlyApiDownloader<Installation>
  {
    private readonly string _installationUri;

    public AirlyInstallationDownloader(
        IConfiguration config, IWebClientAdapter webClientAdapter)
            : base(config, webClientAdapter)
    {
      _installationUri = config.GetValue<string>(
          "AppSettings:AirlyApi:InstallationUri");
    }

    public override async Task<Installation>
        DownloadAirQualityData(short installationId)
    {
      try
      {
        string response = await _webClientAdapter.DownloadStringTaskAsync(
            _installationUri + installationId.ToString());

        return JsonConvert.DeserializeObject<Installation>(
            response,
            new JsonSerializerSettings()
            {
              NullValueHandling = NullValueHandling.Ignore
            })
          ?? new Installation();
      }
      catch (Exception e)
      {
        string stackTrace = e.StackTrace;
        Debug.WriteLine(stackTrace);
        Debug.WriteLine(e.Message);
        Debug.WriteLine(e.Source);
        Debug.WriteLine("\n");

        return new Installation();
      }
    }
  }
}
