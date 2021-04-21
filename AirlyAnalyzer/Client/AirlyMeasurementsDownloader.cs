namespace AirlyAnalyzer.Client
{
  using System;
  using System.Diagnostics;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;

  public class AirlyMeasurementsDownloader : AirlyApiDownloader<Measurements>
  {
    private readonly string _measurementsUri;
    private readonly string _measurementsUriParameters;

    public AirlyMeasurementsDownloader(
        IConfiguration config, IWebClientAdapter webClientAdapter)
            : base(config, webClientAdapter)
    {
      _measurementsUri = config.GetValue<string>(
          "AppSettings:AirlyApi:MeasurementsUri");

      _measurementsUriParameters = config.GetValue<string>(
          "AppSettings:AirlyApi:MeasurementsUriParameters");
    }

    public override async Task<Measurements>
        DownloadAirQualityData(short installationId)
    {
      try
      {
        string response = await _webClientAdapter.DownloadStringTaskAsync(
            _measurementsUri
            + installationId.ToString()
            + _measurementsUriParameters);

        return JsonConvert.DeserializeObject<Measurements>(
            response,
            new JsonSerializerSettings()
            {
              NullValueHandling = NullValueHandling.Ignore
            })
          ?? new Measurements();
      }
      catch (Exception e)
      {
        string stackTrace = e.StackTrace;
        Debug.WriteLine(stackTrace);
        Debug.WriteLine(e.Message);
        Debug.WriteLine(e.Source);
        Debug.WriteLine("\n");

        return new Measurements();
      }
    }
  }
}
