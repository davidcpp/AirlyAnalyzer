namespace AirlyAnalyzer.Tests.ForecastErrorsControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Controllers;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Tests.Fixtures;
  using AirlyAnalyzer.Tests.Models;
  using Microsoft.AspNetCore.Mvc;
  using Xunit;

  [Collection("RepositoryTests")]
  public class IndexTest
  {
    private readonly ForecastErrorsController controller;
    private readonly AirlyContext _context;

    private readonly DateTime _startDate;
    private readonly List<short> _installationIds;

    public IndexTest(RepositoryFixture fixture)
    {
      _context = fixture.Context;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      controller = new ForecastErrorsController(fixture.UnitOfWork);
      _context.Clear();
    }

    [Fact]
    public async Task returns_forecast_errors_from_last_day()
    {
      // Arrange
      const short numberOfDays = 7;
      const short numberOfElementsInDay = 24;

      // In last day there are: hourly, daily and total errors
      int numberOfLastDayErrors
          = (numberOfElementsInDay * _installationIds.Count)
              + (2 * _installationIds.Count) + 1;

      _context.AddElementsToDatabase(
          _installationIds, _startDate, numberOfDays, numberOfElementsInDay);

      // Act
      var response = await controller.Index() as ViewResult;
      var forecastErrors = response.Model as List<AirQualityForecastError>;

      // Assert
      Assert.Equal(numberOfLastDayErrors, forecastErrors.Count);
    }

    [Fact]
    public async Task returns_empty_forecast_error_list_when_no_ones_in_database()
    {
      // Act
      var response = await controller.Index() as ViewResult;
      var forecastErrors = response.Model as List<AirQualityForecastError>;

      // Assert
      Assert.Empty(forecastErrors);
    }
  }
}
