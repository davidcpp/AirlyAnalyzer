namespace AirlyAnalyzer.Tests.Fixtures
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AirlyAnalyzer.Data;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  public class RepositoryFixture : IDisposable
  {
    public RepositoryFixture()
    {
      DateTimeMinValue = new DateTime(2000, 1, 1);
      StartDate = new DateTime(2001, 3, 24, 22, 0, 0, DateTimeKind.Utc);

      var inMemoryDatabaseOptions = new DbContextOptionsBuilder<AirlyContext>()
          .UseInMemoryDatabase("AirlyAnalyzerDbForTesting")
          .Options;

      string configFilePath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

      Config = new ConfigurationBuilder()
          .AddJsonFile(configFilePath)
          .Build();

      InstallationIds = Config
          .GetSection("AppSettings:AirlyApi:InstallationIds")
          .Get<List<short>>();

      Context = new AirlyContext(inMemoryDatabaseOptions, Config);

      UnitOfWork = new UnitOfWork(Context);
    }

    public AirlyContext Context { get; }

    public UnitOfWork UnitOfWork { get; }

    public IConfiguration Config { get; }

    public DateTime DateTimeMinValue { get; }

    public DateTime StartDate { get; }

    public List<short> InstallationIds { get; }

    public void Dispose()
    {
      Context.Dispose();
    }
  }

  [CollectionDefinition("RepositoryTests")]
  public class RepositoryTests : ICollectionFixture<RepositoryFixture>
  {
  }
}
