namespace AirlyAnalyzer.IntegrationTests.ForecastErrorsApiControllerTests
{
  using System.Net.Http;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.IntegrationTests.Extensions;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Xunit;

  public class GetRequestDatesTests
      : IClassFixture<CustomWebApplicationFactory<AirlyAnalyzer.Startup>>
  {
    private readonly CustomWebApplicationFactory<AirlyAnalyzer.Startup> _factory;
    private HttpClient _client;

    public GetRequestDatesTests(
        CustomWebApplicationFactory<AirlyAnalyzer.Startup> factory)
    {
      _factory = factory;
      _client = factory.CreateClient();
    }

    [Fact]
    public async Task returns_success_and_correct_content_type()
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
    public async Task returns_success_and_correct_content_type_when_no_data_in_database()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetRequestDates";
      _client = _factory.WithWebHostBuilder(builder =>
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
                .GetRequiredService<ILogger<GetErrorsInDayTests>>();

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
