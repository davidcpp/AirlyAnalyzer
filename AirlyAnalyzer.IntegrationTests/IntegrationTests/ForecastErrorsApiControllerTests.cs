namespace AirlyAnalyzer.IntegrationTests
{
  using System.Net;
  using System.Net.Http;
  using System.Threading.Tasks;
  using Xunit;

  public class ForecastErrorsApiControllerTests
      : IClassFixture<CustomWebApplicationFactory<AirlyAnalyzer.Startup>>
  {
    private readonly HttpClient _client;

    public ForecastErrorsApiControllerTests(
        CustomWebApplicationFactory<AirlyAnalyzer.Startup> factory)
    {
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
    public async Task GetErrorsInDayAsync_WhenIncorrectDate_ReturnsBadRequest()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetErrorsInDay/2000";

      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert   
      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRequestDatesAsync_ReturnSuccessAndCorrectContentType()
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
