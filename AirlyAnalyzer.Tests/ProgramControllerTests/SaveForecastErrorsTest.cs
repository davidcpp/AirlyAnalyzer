namespace AirlyAnalyzer.Tests.ProgramControllerTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Services;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Tests.Fixtures;
  using AirlyAnalyzer.Tests.Models;
  using static AirlyAnalyzer.Tests.Models.AuxiliaryMethods;
  using Xunit;

  [Collection("RepositoryTests")]
  public class SaveForecastErrorsTest
  {
    private readonly ProgramController _programController;
    private readonly AirlyContext _context;
    private readonly DateTime _startDate;
    private readonly List<short> _installationIds;

    public SaveForecastErrorsTest(RepositoryFixture fixture)
    {
      _context = fixture.Context;
      _startDate = fixture.StartDate;
      _installationIds = fixture.InstallationIds;

      _programController = new ProgramController(fixture.UnitOfWork);

      _context.Clear();
    }

    [Theory]
    [InlineData(7, 0)]
    [InlineData(7, 3)]
    public async Task returns_daily_foreacst_errors_count(
        short numberOfHourlyErrorDays, short numberOfDailyErrorDays)
    {
      // Arrange
      const short numberOfElementsInDay = 24;

      int numberOfDailyErrors = numberOfDailyErrorDays * _installationIds.Count;

      var hourlyForecastErrors = new List<AirQualityForecastError>();
      var dailyForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIds)
      {
        hourlyForecastErrors.AddRange(GenerateHourlyForecastErrors(
            installationId,
            _startDate,
            numberOfHourlyErrorDays,
            numberOfElementsInDay));

        dailyForecastErrors.AddRange(GenerateDailyForecastErrors(
            installationId,
            _startDate,
            numberOfDailyErrorDays,
            numberOfElementsInDay));
      }

      // Act 
      int result = await _programController
          .SaveForecastErrors(hourlyForecastErrors, dailyForecastErrors);

      // Assert
      Assert.Equal(numberOfDailyErrors, result);
    }

    [Fact]
    public async Task do_not_save_any_forecast_errors_in_database_when_get_empty_new_ones()
    {
      // Arrange
      var hourlyForecastErrors = new List<AirQualityForecastError>();
      var dailyForecastErrors = new List<AirQualityForecastError>();

      // Act 
      await _programController
          .SaveForecastErrors(hourlyForecastErrors, dailyForecastErrors);

      // Assert
      Assert.Empty(_context.ForecastErrors);
    }

    [Fact]
    public async Task save_new_forecast_errors_in_database_when_get_new_ones()
    {
      // Arrange
      const short numberOfHourlyErrorDays = 7;
      const short numberOfDailyErrorDays = 3;
      const short numberOfElementsInDay = 24;

      int numberOfHourlyErrors
          = numberOfHourlyErrorDays * numberOfElementsInDay * _installationIds.Count;

      int numberOfDailyErrors
          = numberOfDailyErrorDays * _installationIds.Count;

      var hourlyForecastErrors = new List<AirQualityForecastError>();
      var dailyForecastErrors = new List<AirQualityForecastError>();

      foreach (short installationId in _installationIds)
      {
        hourlyForecastErrors.AddRange(GenerateHourlyForecastErrors(
            installationId, _startDate, numberOfHourlyErrorDays, numberOfElementsInDay));

        dailyForecastErrors.AddRange(GenerateDailyForecastErrors(
            installationId, _startDate,numberOfDailyErrorDays, numberOfElementsInDay));
      }

      // Act 
      await _programController
          .SaveForecastErrors(hourlyForecastErrors, dailyForecastErrors);

      // Assert
      Assert.Equal(
        numberOfHourlyErrors,
        _context.ForecastErrors.Where(
            fe => fe.ErrorType == ForecastErrorType.Hourly).Count());

      Assert.Equal(
        numberOfDailyErrors,
        _context.ForecastErrors.Where(
            fe => fe.ErrorType == ForecastErrorType.Daily).Count());
    }
  }
}
