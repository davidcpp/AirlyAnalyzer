namespace AirlyAnalyzer.IntegrationTests.ForecastErrorsControllerTests
{
  using System.Net.Http;
  using System.Threading.Tasks;
  using Xunit;

  [Collection("ControllerIntegrationTests")]
  public class IndexTests
  {
    private readonly HttpClient _client;

    public IndexTests(
        CustomWebApplicationFactory<AirlyAnalyzer.Startup> factory)
    {
      _client = factory.CreateClient();
    }

    [Fact]
    public async Task returns_success_and_correct_content_type()
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
