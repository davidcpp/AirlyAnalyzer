﻿namespace AirlyAnalyzer.IntegrationTests
{
  using System;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.IntegrationTests.Extensions;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Xunit;

  public class CustomWebApplicationFactory<TStartup>
      : WebApplicationFactory<TStartup> where TStartup : class
  {
    public CustomWebApplicationFactory()
    {
      NumberOfDays = 10;
    }

    public short NumberOfDays { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.ConfigureServices(services =>
      {
        services.ConfigureCommonServices();

        var sp = services.BuildServiceProvider();

        using (var scope = sp.CreateScope())
        {
          var scopedServices = scope.ServiceProvider;
          var context = scopedServices.GetRequiredService<AirlyContext>();
          var logger = scopedServices
              .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

          context.Database.EnsureCreated();

          try
          {
            context.Clear();
            context.Seed(NumberOfDays);
          }
          catch (Exception ex)
          {
            logger.LogError(ex, "An error occurred seeding or clearing the " +
                "database with test messages. Error: {Message}", ex.Message);
          }
        }
      });
    }
  }

  [CollectionDefinition("ControllerIntegrationTests")]
  public class ControllerIntegrationTests
      : ICollectionFixture<CustomWebApplicationFactory<AirlyAnalyzer.Startup>>
  {
  }
}
