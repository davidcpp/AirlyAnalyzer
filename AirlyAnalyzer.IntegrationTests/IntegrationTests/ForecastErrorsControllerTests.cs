namespace AirlyAnalyzer.IntegrationTests
{
  using System.Net.Http;
  using System.Threading.Tasks;
  using Xunit;

  public class ForecastErrorsControllerTests
      : IClassFixture<CustomWebApplicationFactory<AirlyAnalyzer.Startup>>
  {
    private readonly HttpClient _client;

    public ForecastErrorsControllerTests(
        CustomWebApplicationFactory<AirlyAnalyzer.Startup> factory)
    {
      _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAsync_Index_ReturnsSuccessAndCorrectContentType()
    {
      // Arrange
      const string requestUrl = "";

      // Act
      var response = await _client.GetAsync(requestUrl);

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal("text/html; charset=utf-8",
          response.Content.Headers.ContentType.ToString().ToLower());
    }
  }
}
