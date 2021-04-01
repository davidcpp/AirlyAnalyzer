namespace AirlyAnalyzer.UnitTests.Helpers
{
  using System;
  using System.IO;
  using Microsoft.Extensions.Configuration;

  public static class ConfigUtilities
  {
    private static readonly IConfiguration _config;

    static ConfigUtilities()
    {
      string configFilePath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

      _config = new ConfigurationBuilder()
          .AddJsonFile(configFilePath)
          .Build();
    }

    public static IConfiguration GetApplicationConfig()
    {
      return _config;
    }
  }
}
