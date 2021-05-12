// This file was auto-generated by ML.NET Model Builder. 

using System;
using AirlyAnalyzerML.Model;

namespace AirlyAnalyzerML.ConsoleApp
{
  class Program
  {
    static void Main(string[] args)
    {
      // Create single instance of sample data from first line of dataset for model input
      ModelInput sampleData = new ModelInput()
      {
        Month = 3F,
        Day = 27F,
        Hour = 23F,
        InstallationId = 18730F,
        Humidity = 64F,
        Temperature = @"3,6",
        Visibility = 6000F,
        WindSpeed = 2F,
      };

      // Make a single prediction on the sample data and print results
      var predictionResult = ConsumeModel.Predict(sampleData);

      Console.WriteLine("Using model to make single prediction -- Comparing actual AirlyCaqi with predicted AirlyCaqi from sample data...\n\n");
      Console.WriteLine($"Month: {sampleData.Month}");
      Console.WriteLine($"Day: {sampleData.Day}");
      Console.WriteLine($"Hour: {sampleData.Hour}");
      Console.WriteLine($"InstallationId: {sampleData.InstallationId}");
      Console.WriteLine($"Humidity: {sampleData.Humidity}");
      Console.WriteLine($"Temperature: {sampleData.Temperature}");
      Console.WriteLine($"Visibility: {sampleData.Visibility}");
      Console.WriteLine($"WindSpeed: {sampleData.WindSpeed}");
      Console.WriteLine($"\n\nPredicted AirlyCaqi: {predictionResult.Score}\n\n");
      Console.WriteLine("=============== End of process, hit any key to finish ===============");
      Console.ReadKey();
    }
  }
}
