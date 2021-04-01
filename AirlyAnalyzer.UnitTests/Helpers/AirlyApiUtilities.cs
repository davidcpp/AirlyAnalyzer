namespace AirlyAnalyzer.UnitTests.Helpers
{
  using AirlyAnalyzer.Models;
  using Newtonsoft.Json;

  static public class AirlyApiUtilities
  {
    static public string GetTestAirlyInstallationJson()
    {
      return @"{
        ""id"": 6310,
        ""location"": {
            ""latitude"": 52.506371,
            ""longitude"": 16.257352
        },
        ""locationId"": 6310,
        ""address"": {
            ""country"": ""Poland"",
            ""city"": ""Pniewy"",
            ""street"": ""Poznańska"",
            ""number"": ""11a"",
            ""displayAddress1"": ""Pniewy"",
            ""displayAddress2"": ""Poznańska""
        },
        ""elevation"": 98.03,
        ""airly"": true,
        ""sponsor"": {
            ""id"": 350,
            ""name"": ""Pniewy"",
            ""description"": ""Airly Sensor's sponsor"",
            ""logo"": ""https://cdn.airly.eu/logo/Pniewy_1539543816447_2036842331.jpg"",
            ""link"": null,
            ""displayName"": ""Pniewy""
        }
        }";
    }
  }
}
