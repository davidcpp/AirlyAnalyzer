namespace AirlyAnalyzer.Utilities
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Data;

  public static class WeatherUtilities
  {
    public static async Task AddCaqiToWeatherMeasurements(
        this UnitOfWork unitOfWork, List<short> installationIds)
    {
      const short initialInstallationId = 18730;

      var weatherMeasurements
          = await unitOfWork.WeatherMeasurementsRepository.Get(
              wherePredicate: wm => wm.InstallationId == initialInstallationId);

      foreach (short installationId in installationIds)
      {
        if (installationId != initialInstallationId)
        {
          // Add weatherMeasurements with changed InstallationId only,
          // no AirlyCaqi change - faulty - SQL code fix it
          weatherMeasurements.ForEach(wm => wm.InstallationId = installationId);

          await unitOfWork.WeatherMeasurementsRepository
              .AddListAsync(weatherMeasurements);

          await unitOfWork.SaveChangesAsync();
        }
      }

      weatherMeasurements
          = await unitOfWork.WeatherMeasurementsRepository.Get();

      foreach (var weatherMeasurement in weatherMeasurements)
      {
        var airQualityTillDateTime = new DateTime(
            weatherMeasurement.Year,
            weatherMeasurement.Month,
            weatherMeasurement.Day,
            weatherMeasurement.Hour,
            0,
            0,
            DateTimeKind.Utc);

        var airQualityFromDateTime = airQualityTillDateTime.AddHours(-1);

        var airQualityMeasurement = await unitOfWork
            .MeasurementRepository.GetById(
                airQualityTillDateTime,
                airQualityFromDateTime,
                (short)weatherMeasurement.InstallationId);

        if (airQualityMeasurement != null && airQualityMeasurement.AirlyCaqi != 0)
        {
          weatherMeasurement.AirlyCaqi = airQualityMeasurement.AirlyCaqi;
        }
        else
        {
          await unitOfWork.WeatherMeasurementsRepository.Delete(
              weatherMeasurement.Year,
              weatherMeasurement.Month,
              weatherMeasurement.Day,
              weatherMeasurement.Hour,
              weatherMeasurement.InstallationId);
        }
      }

      await unitOfWork.SaveChangesAsync();
    }
  }
}
