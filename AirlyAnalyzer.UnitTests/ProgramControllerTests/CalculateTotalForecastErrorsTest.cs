namespace AirlyAnalyzer.UnitTests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Calculation;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.UnitTests.Fixtures;
  using AirlyAnalyzer.UnitTests.Helpers;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  [Collection("RepositoryTests")]
  public class CalculateTotalForecastErrorsTest
  {
    private readonly IForecastErrorsCalculator _forecastErrorsCalculator;
    private readonly UnitOfWork _unitOfWork;
    private readonly AirlyContext _context;

    private readonly DateTime _startDate;
    private readonly List<short> _installationIds;

    private readonly short _minNumberOfMeasurements;
    private readonly short _idForAllInstallations;

    public CalculateTotalForecastErrorsTest(RepositoryFixture fixture)
    {
      _unitOfWork = fixture.UnitOfWork;
      _context = fixture.Context;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      _minNumberOfMeasurements = fixture.Config.GetValue<short>(
          "AppSettings:AirlyApi:MinNumberOfMeasurements");

      _idForAllInstallations = fixture.Config.GetValue<short>(
          "AppSettings:AirlyApi:IdForAllInstallations");

      _forecastErrorsCalculator = new ForecastPlainErrorsCalculator();

      _context.Clear();
    }

    [Fact]
    public async Task returns_empty_new_total_error_list_when_no_installations()
    {
      // Arrange
      const short numberOfDays = 2;
      var installationIds = new List<short>();

      _context.AddAllElementsToDatabase(
          _installationIds, _startDate, numberOfDays, _minNumberOfMeasurements);

      var programController = new ProgramController(
          _unitOfWork, _forecastErrorsCalculator, installationIds);

      // Act
      var newTotalForecastErrors
          = await programController.CalculateTotalForecastErrors();

      // Assert
      Assert.Empty(newTotalForecastErrors);
    }

    [Fact]
    public async Task returns_empty_new_total_error_list_when_no_daily_errors_in_database()
    {
      // Arrange
      const short numberOfDays = 1;
      const short numberOfElements = 22;

      _context.AddAllMeasurementsToDatabase(
          _installationIds, _startDate, numberOfDays, numberOfElements);

      _context.AddAllForecastsToDatabase(
          _installationIds, _startDate, numberOfDays, numberOfElements);

      foreach (short installationId in _installationIds)
      {
        _context.AddHourlyForecastErrorsToDatabase(
            installationId, _startDate, numberOfElements);
      }

      var programController = new ProgramController(
          _unitOfWork, _forecastErrorsCalculator, _installationIds);

      // Act
      var newTotalForecastErrors
          = await programController.CalculateTotalForecastErrors();

      // Assert
      Assert.Empty(newTotalForecastErrors);
    }

    [Fact]
    public async Task returns_correct_new_total_error_list()
    {
      // Arrange
      const short numberOfDays = 2;

      _context.AddAllElementsToDatabase(
          _installationIds, _startDate, numberOfDays, _minNumberOfMeasurements);

      var programController = new ProgramController(
          _unitOfWork,
          _forecastErrorsCalculator,
          _installationIds,
          _idForAllInstallations);

      // Act
      var newTotalForecastErrors
          = await programController.CalculateTotalForecastErrors();

      // Assert
      Assert.Equal(_installationIds.Count + 1, newTotalForecastErrors.Count);
      Assert.Equal(
          _idForAllInstallations,
          newTotalForecastErrors.Last().InstallationId);
    }
  }
}
