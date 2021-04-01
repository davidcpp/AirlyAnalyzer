namespace AirlyAnalyzer.Tests.Fixtures
{
  using System;
  using System.Collections.Generic;
  using AirlyAnalyzer.Tests.Helpers;
  using Microsoft.Extensions.Configuration;

  public class SimpleFixture
  {
    public SimpleFixture()
    {
      StartDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);
      InstallationId = 1;

      var config = ConfigUtilities.GetApplicationConfig();

      InstallationIds = config
          .GetSection("AppSettings:AirlyApi:InstallationIds")
          .Get<List<short>>();
    }

    public DateTime StartDate { get; }

    public List<short> InstallationIds { get; }

    public short InstallationId { get; }
  }
}
