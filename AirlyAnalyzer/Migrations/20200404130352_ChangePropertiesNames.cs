using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
  public partial class ChangePropertiesNames : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "AirlyCaqiAccuracy",
          table: "ForecastAccuracyRates");

      migrationBuilder.DropColumn(
          name: "ForecastRequestDate",
          table: "ForecastAccuracyRates");

      migrationBuilder.DropColumn(
          name: "Pm10Accuracy",
          table: "ForecastAccuracyRates");

      migrationBuilder.DropColumn(
          name: "Pm25Accuracy",
          table: "ForecastAccuracyRates");

      migrationBuilder.DropColumn(
          name: "AirlyCaqiValue",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "Pm10Value",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "Pm25Value",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "RequestTime",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "AirlyCaqiValue",
          table: "ArchiveForecasts");

      migrationBuilder.DropColumn(
          name: "Pm10Value",
          table: "ArchiveForecasts");

      migrationBuilder.DropColumn(
          name: "Pm25Value",
          table: "ArchiveForecasts");

      migrationBuilder.DropColumn(
          name: "RequestTime",
          table: "ArchiveForecasts");

      migrationBuilder.AddColumn<double>(
          name: "AirlyCaqiError",
          table: "ForecastAccuracyRates",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<DateTime>(
          name: "ForecastRequestDateTime",
          table: "ForecastAccuracyRates",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

      migrationBuilder.AddColumn<double>(
          name: "Pm10Error",
          table: "ForecastAccuracyRates",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm25Error",
          table: "ForecastAccuracyRates",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "AirlyCaqi",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm10",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm25",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<DateTime>(
          name: "RequestDateTime",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

      migrationBuilder.AddColumn<double>(
          name: "AirlyCaqi",
          table: "ArchiveForecasts",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm10",
          table: "ArchiveForecasts",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm25",
          table: "ArchiveForecasts",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<DateTime>(
          name: "RequestDateTime",
          table: "ArchiveForecasts",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "AirlyCaqiError",
          table: "ForecastAccuracyRates");

      migrationBuilder.DropColumn(
          name: "ForecastRequestDateTime",
          table: "ForecastAccuracyRates");

      migrationBuilder.DropColumn(
          name: "Pm10Error",
          table: "ForecastAccuracyRates");

      migrationBuilder.DropColumn(
          name: "Pm25Error",
          table: "ForecastAccuracyRates");

      migrationBuilder.DropColumn(
          name: "AirlyCaqi",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "Pm10",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "Pm25",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "RequestDateTime",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "AirlyCaqi",
          table: "ArchiveForecasts");

      migrationBuilder.DropColumn(
          name: "Pm10",
          table: "ArchiveForecasts");

      migrationBuilder.DropColumn(
          name: "Pm25",
          table: "ArchiveForecasts");

      migrationBuilder.DropColumn(
          name: "RequestDateTime",
          table: "ArchiveForecasts");

      migrationBuilder.AddColumn<double>(
          name: "AirlyCaqiAccuracy",
          table: "ForecastAccuracyRates",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<DateTime>(
          name: "ForecastRequestDate",
          table: "ForecastAccuracyRates",
          type: "datetime2",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

      migrationBuilder.AddColumn<double>(
          name: "Pm10Accuracy",
          table: "ForecastAccuracyRates",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm25Accuracy",
          table: "ForecastAccuracyRates",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "AirlyCaqiValue",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm10Value",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm25Value",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<DateTime>(
          name: "RequestTime",
          table: "ArchiveMeasurements",
          type: "datetime2",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

      migrationBuilder.AddColumn<double>(
          name: "AirlyCaqiValue",
          table: "ArchiveForecasts",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm10Value",
          table: "ArchiveForecasts",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm25Value",
          table: "ArchiveForecasts",
          type: "float",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<DateTime>(
          name: "RequestTime",
          table: "ArchiveForecasts",
          type: "datetime2",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
    }
  }
}
