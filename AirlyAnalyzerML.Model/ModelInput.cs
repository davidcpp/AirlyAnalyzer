// This file was auto-generated by ML.NET Model Builder. 

using Microsoft.ML.Data;

namespace AirlyAnalyzerML.Model
{
  public class ModelInput
  {
    [ColumnName("Year"), LoadColumn(0)]
    public short Year { get; set; }

    [ColumnName("Month"), LoadColumn(1)]
    public byte Month { get; set; }

    [ColumnName("Day"), LoadColumn(2)]
    public byte Day { get; set; }

    [ColumnName("Hour"), LoadColumn(3)]
    public byte Hour { get; set; }

    [ColumnName("InstallationId"), LoadColumn(4)]
    public int InstallationId { get; set; }

    [ColumnName("CloudCover"), LoadColumn(5)]
    public byte CloudCover { get; set; }

    [ColumnName("Humidity"), LoadColumn(6)]
    public byte Humidity { get; set; }

    [ColumnName("Pressure"), LoadColumn(7)]
    public float Pressure { get; set; }

    [ColumnName("Rain6h"), LoadColumn(8)]
    public float Rain6h { get; set; }

    [ColumnName("Temperature"), LoadColumn(9)]
    public float Temperature { get; set; }

    [ColumnName("Visibility"), LoadColumn(10)]
    public int Visibility { get; set; }

    [ColumnName("WindSpeed"), LoadColumn(11)]
    public float WindSpeed { get; set; }

    [ColumnName("WindDirection"), LoadColumn(12)]
    public short WindDirection { get; set; }

    [ColumnName("WindGust"), LoadColumn(13)]
    public float WindGust { get; set; }

    [ColumnName("AirlyCaqi"), LoadColumn(14)]
    public short AirlyCaqi { get; set; }
  }
}
