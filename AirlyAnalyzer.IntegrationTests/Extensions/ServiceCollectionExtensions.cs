namespace AirlyAnalyzer.IntegrationTests.Extensions
{
  using System.Linq;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.DependencyInjection;
  using Moq;

  public static class ServiceCollectionExtensions
  {
    public static void ConfigureCommonServices(this IServiceCollection services)
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

      services.AddSingleton(_ => airlyDataDownloaderMock.Object);
    }
  }
}
