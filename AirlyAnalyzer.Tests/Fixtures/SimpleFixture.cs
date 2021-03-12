namespace AirlyAnalyzer.Tests.Fixtures
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using Microsoft.Extensions.Configuration;

  public class SimpleFixture
  {
    public SimpleFixture()
    {
      StartDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);
      InstallationId = 1;

      string configFilePath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

      var config = new ConfigurationBuilder()
          .AddJsonFile(configFilePath)
          .Build();

      InstallationIds = config
          .GetSection("AppSettings:AirlyApi:InstallationIds")
          .Get<List<short>>();
    }

    public DateTime StartDate { get; }

    public List<short> InstallationIds { get; }

    public short InstallationId { get; }
  }
}
