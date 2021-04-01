namespace AirlyAnalyzer.UnitTests.ForecastErrorsApiControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Controllers;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Xunit;

  [Collection("RepositoryTests")]
  public class GetRequestDatesTest
  {
    private readonly ForecastErrorsApiController controller;
    private readonly AirlyContext _context;

    private readonly DateTime _startDate;
    private readonly List<short> _installationIds;

    public GetRequestDatesTest(RepositoryFixture fixture)
    {
      _context = fixture.Context;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      controller = new ForecastErrorsApiController(fixture.UnitOfWork);
      _context.Clear();
    }

    [Fact]
    public async Task returns_request_dates()
    {
      // Arrange
      const short numberOfDays = 17;
      const short numberOfElementsInDay = 24;

      _context.AddAllElementsToDatabase(
          _installationIds, _startDate, numberOfDays, numberOfElementsInDay);

      // Act
      var requestDates = await controller.GetRequestDates();

      // Assert
      Assert.Equal(numberOfDays, requestDates.Count);
    }

    [Fact]
    public async Task returns_empty_request_date_list_when_no_forecast_errors_in_database()
    {
      // Act
      var requestDates = await controller.GetRequestDates();

      // Assert
      Assert.Empty(requestDates);
    }
  }
}
