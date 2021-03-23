namespace AirlyAnalyzer.IntegrationTests
{
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Moq;
  using Xunit;

  public class ForecastErrorsApiControllerTests
      : IClassFixture<CustomWebApplicationFactory<AirlyAnalyzer.Startup>>
  {
    private readonly CustomWebApplicationFactory<AirlyAnalyzer.Startup> _factory;
    private HttpClient _client;

    public ForecastErrorsApiControllerTests(
        CustomWebApplicationFactory<AirlyAnalyzer.Startup> factory)
    {
      _factory = factory;
      _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetErrorsInDayAsync_ReturnsSuccessAndCorrectContentType()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetErrorsInDay/2000-01-01";

      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal("application/json; charset=utf-8",
          response.Content.Headers.ContentType.ToString().ToLower());
    }

    [Fact]
    public async Task GetErrorsInDayAsync_ReturnsBadRequest_WhenIncorrectDate()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetErrorsInDay/2000";

      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert   
      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetErrorsInDayAsync_ReturnsNotFound_WhenNoForecastErrorsInDay()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetErrorsInDay/1000-01-01";

      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert
      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRequestDatesAsync_ReturnsSuccessAndCorrectContentType()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetRequestDates";

      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal("application/json; charset=utf-8",
          response.Content.Headers.ContentType.ToString().ToLower());
    }

    [Fact]
    public async Task GetRequestDatesAsync_ReturnsSuccessAndCorrectContentType_WhenNoDataInDatabase()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetRequestDates";
      _client = _factory.WithWebHostBuilder(builder =>
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
                        .GetRequiredService<ILogger<ForecastErrorsApiControllerTests>>();

                    context.Database.EnsureCreated();
                  }
                });
          })
          .CreateClient();
      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal("application/json; charset=utf-8",
          response.Content.Headers.ContentType.ToString().ToLower());
    }
  }
}
