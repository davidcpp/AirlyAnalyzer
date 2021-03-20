namespace AirlyAnalyzer.Tests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Controllers;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Tests.Fixtures;
  using AirlyAnalyzer.Tests.Models;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Xunit;

  using ET = AirlyAnalyzer.Models.ForecastErrorType;

  [Collection("RepositoryTests")]
  public class UpdateTotalForecastErrorsTest
  {
    private readonly ProgramController programController;
    private readonly AirlyContext _context;
    private readonly DateTime _startDate;
    private readonly List<short> _installationIds;

    public UpdateTotalForecastErrorsTest(RepositoryFixture fixture)
    {
      _context = fixture.Context;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      programController = new ProgramController(fixture.UnitOfWork);

      _context.Clear();
    }

    [Fact]
    public async Task do_not_remove_total_forecast_errors_when_no_new_total_errors()
    {
      // Arrange
      const short numberOfDays = 10;

      _context.AddTotalForecastErrorsToDatabase(
          _installationIds, _startDate, numberOfDays);

      var newTotalForecastErrors = new List<AirQualityForecastError>();

      // Act
      await programController.UpdateTotalForecastErrors(newTotalForecastErrors);

      // Assert
      Assert.Equal(_installationIds.Count + 1, _context.ForecastErrors.Count());
    }

    [Fact]
    public async Task replace_total_forecast_errors_when_new_total_errors()
    {
      // Arrange
      const short numberOfDays = 10;
      const short newNumberOfDays = 12;

      _context.AddTotalForecastErrorsToDatabase(
          _installationIds, _startDate, numberOfDays);

      var newTotalForecastErrors = GenerateTotalForecastErrors(
          _installationIds, _startDate, newNumberOfDays)
        .ToList();

      // Act
      await programController.UpdateTotalForecastErrors(newTotalForecastErrors);

      // Assert
      Assert.Equal(_installationIds.Count + 1, _context.ForecastErrors.Count());

      for (int i = 0; i < newTotalForecastErrors.Count; i++)
      {
        Assert.Equal(
            newTotalForecastErrors[i].RequestDateTime,
            _context.ForecastErrors.ToList()[i].RequestDateTime);
      }
    }
  }
}
