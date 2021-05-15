namespace AirlyAnalyzer.UnitTests.Helpers
{
  using System;
  using System.IO;
  using System.Reflection;

  static public class OpenWeatherApiUtilities
  {
    static public string GetTestOpenWeatherApiForecastJson()
    {
      string currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

      string solutionFolder = Environment.CurrentDirectory;

      solutionFolder = solutionFolder.Substring(
          0,
          solutionFolder.IndexOf(currentAssemblyName)
          + currentAssemblyName.Length);

      string forecastJsonFilepath = Path.Combine(
          solutionFolder,
          @"Helpers\Data",
          "OpenWeatherForecastExample.json");

      using (var file = new StreamReader(forecastJsonFilepath))
      {
        return file.ReadToEnd();
      }
    }
  }
}
