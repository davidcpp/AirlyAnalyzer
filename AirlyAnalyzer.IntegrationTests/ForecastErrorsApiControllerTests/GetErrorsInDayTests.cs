namespace AirlyAnalyzer.IntegrationTests.ForecastErrorsApiControllerTests
{
  using System.Net;
  using System.Net.Http;
  using System.Threading.Tasks;
  using Xunit;

  public class GetErrorsInDayTests
      : IClassFixture<CustomWebApplicationFactory<AirlyAnalyzer.Startup>>
  {
    private readonly HttpClient _client;

    public GetErrorsInDayTests(
        CustomWebApplicationFactory<AirlyAnalyzer.Startup> factory)
    {
      _client = factory.CreateClient();
    }

    [Fact]
    public async Task returns_success_and_correct_content_type()
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
    public async Task returns_bad_request_when_incorrect_date()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetErrorsInDay/2000";

      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert   
      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task returns_not_found_when_no_forecast_errors_in_day()
    {
      // Arrange
      const string requestUrl = "/api/ForecastErrorsApi/GetErrorsInDay/1000-01-01";

      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert
      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
  }
}
