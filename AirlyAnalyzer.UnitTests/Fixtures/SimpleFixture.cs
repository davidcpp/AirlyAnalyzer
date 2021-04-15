namespace AirlyAnalyzer.UnitTests.Fixtures
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Microsoft.Extensions.Configuration;

  public class SimpleFixture
  {
    public SimpleFixture()
    {
      StartDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);
      InstallationId = 1;

      Config = ConfigUtilities.GetApplicationConfig();

      InstallationIds = Config
          .GetSection("AppSettings:AirlyApi:InstallationIds")
          .Get<List<short>>();
    }

    public IConfiguration Config { get; }

    public DateTime StartDate { get; }

    public List<short> InstallationIds { get; }

    public short InstallationId { get; }
  }
}
