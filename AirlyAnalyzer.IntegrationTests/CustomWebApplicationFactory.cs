namespace AirlyAnalyzer.IntegrationTests
{
  using System;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.IntegrationTests.Extensions;
  using AirlyAnalyzer.Tests.Models;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;

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
            context.Seed(NumberOfDays);
          }
          catch (Exception ex)
          {
            logger.LogError(ex, "An error occurred seeding the " +
                "database with test messages. Error: {Message}", ex.Message);
          }
        }
      });
    }
  }
}
