namespace AirlyAnalyzer.Tests.Fixtures
{
  using System;

  public class SimpleFixture
  {
    public SimpleFixture()
    {
      StartDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);
      InstallationId = 1;
    }

    public DateTime StartDate { get; }

    public short InstallationId { get; }
  }
}
