namespace AirlyAnalyzer.IntegrationTests.ForecastErrorsApiControllerTests
{
  using System.Net.Http;
  using System.Threading.Tasks;
  using Xunit;

  [Collection("ControllerIntegrationTests")]
  public class GetRequestDatesTests
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
  }
}
