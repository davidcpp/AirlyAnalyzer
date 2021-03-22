namespace AirlyAnalyzer.IntegrationTests
{
  using System;
  using System.Linq;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Tests.Models;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Moq;

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
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType ==
                typeof(DbContextOptions<AirlyContext>));

        services.Remove(descriptor);

        services.AddDbContext<AirlyContext>(options =>
        {
          options.UseInMemoryDatabase("AirlyAnalyzerDbForTesting");
        });

        var airlyDataDownloaderMock
            = new Mock<IAirQualityDataDownloader<Measurements>>();

        airlyDataDownloaderMock
            .Setup(x => x.DownloadAirQualityData(It.IsAny<short>()))
            .ReturnsAsync(new Measurements());

        services.AddScoped(_ => airlyDataDownloaderMock.Object);

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
