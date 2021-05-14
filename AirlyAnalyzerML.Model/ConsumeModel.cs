// This file was auto-generated by ML.NET Model Builder. 

using System;
using System.IO;
using Microsoft.ML;

namespace AirlyAnalyzerML.Model
{
  public static class ConsumeModel
  {
    private static Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictionEngine
        = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(
            CreatePredictionEngine);

    // For more info on consuming ML.NET models, visit https://aka.ms/mlnet-consume
    // Method for consuming model in your app
    public static ModelOutput Predict(ModelInput input)
    {
      var result = PredictionEngine.Value.Predict(input);
      return result;
    }

    public static PredictionEngine<ModelInput, ModelOutput> CreatePredictionEngine()
    {
      // Create new MLContext
      var mlContext = new MLContext();

      string solutionFolder = Environment.CurrentDirectory;

      solutionFolder = solutionFolder.Substring(
          0, solutionFolder.IndexOf(AppDomain.CurrentDomain.FriendlyName));

      // Load model & create prediction engine
      string modelPath = Path.Combine(
          solutionFolder, "AirlyAnalyzerML.Model", "MLModel.zip");

      ITransformer mlModel = mlContext.Model
          .Load(modelPath, out var _);

      var predEngine = mlContext.Model
          .CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

      return predEngine;
    }
  }
}