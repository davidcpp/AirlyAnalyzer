namespace AirlyAnalyzer.Tests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Calculation;
  using AirlyAnalyzer.Controllers;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Tests.Fixtures;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Microsoft.Extensions.Configuration;
  using Xunit;

  [Collection("RepositoryTests")]
  public class CalculateTotalForecastErrorsTest
  {
    private IForecastErrorsCalculator _forecastErrorsCalculation;
    private readonly UnitOfWork _unitOfWork;
    private readonly AirlyContext _context;

    private readonly DateTime _startDate;
    private List<short> _installationIds;

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

      _forecastErrorsCalculation =
          new ForecastErrorsCalculator(_minNumberOfMeasurements);

      Seed(_context);
    }

    [Fact]
    public async Task empty_new_total_errors_when_no_installations()
    {
      // Arrange
      const short numberOfDays = 2;
      var installationIds = new List<short>();

      _context.AddElementsToDatabase(
          _installationIds, _startDate, numberOfDays, _minNumberOfMeasurements);

      var programController = new ProgramController(
          _unitOfWork, _forecastErrorsCalculation, installationIds);

      // Act
      var newTotalForecastErrors
          = await programController.CalculateTotalForecastErrors();

      // Assert
      Assert.Empty(newTotalForecastErrors);
    }

    [Fact]
    public async Task empty_new_total_errors_when_no_daily_errors_in_database()
    {
      // Arrange
      var programController = new ProgramController(
          _unitOfWork, _forecastErrorsCalculation, _installationIds);

      // Act
      var newTotalForecastErrors
          = await programController.CalculateTotalForecastErrors();

      // Assert
      Assert.Empty(newTotalForecastErrors);
    }

    [Fact]
    public async Task proper_new_total_errors_when_one_daily_error_for_each_installation()
    {
      // Arrange
      const short numberOfDays = 2;

      _context.AddElementsToDatabase(
          _installationIds, _startDate, numberOfDays, _minNumberOfMeasurements);

      var programController = new ProgramController(
          _unitOfWork,
          _forecastErrorsCalculation,
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
