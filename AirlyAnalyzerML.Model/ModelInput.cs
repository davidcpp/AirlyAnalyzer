// This file was auto-generated by ML.NET Model Builder. 

using Microsoft.ML.Data;

namespace AirlyAnalyzerML.Model
{
  public class ModelInput
  {
    [ColumnName("Year"), LoadColumn(0)]
    public float Year { get; set; }


    [ColumnName("Month"), LoadColumn(1)]
    public float Month { get; set; }


    [ColumnName("Day"), LoadColumn(2)]
    public float Day { get; set; }


    [ColumnName("Hour"), LoadColumn(3)]
    public float Hour { get; set; }


    [ColumnName("InstallationId"), LoadColumn(4)]
    public float InstallationId { get; set; }


    [ColumnName("CloudCover"), LoadColumn(5)]
    public float CloudCover { get; set; }


    [ColumnName("Humidity"), LoadColumn(6)]
    public float Humidity { get; set; }


    [ColumnName("Pressure"), LoadColumn(7)]
    public string Pressure { get; set; }


    [ColumnName("Rain6h"), LoadColumn(8)]
    public string Rain6h { get; set; }


    [ColumnName("Temperature"), LoadColumn(9)]
    public string Temperature { get; set; }


    [ColumnName("Visibility"), LoadColumn(10)]
    public float Visibility { get; set; }


    [ColumnName("WindSpeed"), LoadColumn(11)]
    public float WindSpeed { get; set; }


    [ColumnName("WindDirection"), LoadColumn(12)]
    public float WindDirection { get; set; }


    [ColumnName("WindGust"), LoadColumn(13)]
    public float WindGust { get; set; }


    [ColumnName("AirlyCaqi"), LoadColumn(14)]
    public float AirlyCaqi { get; set; }
  }
}
