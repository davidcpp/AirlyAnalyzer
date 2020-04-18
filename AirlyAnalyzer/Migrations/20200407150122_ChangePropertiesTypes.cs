using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
  public partial class ChangePropertiesTypes : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<short>(
          name: "Pm25Error",
          table: "ForecastAccuracyRates",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "Pm10Error",
          table: "ForecastAccuracyRates",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "AirlyCaqiError",
          table: "ForecastAccuracyRates",
          nullable: false,
          oldClrType: typeof(byte),
          oldType: "tinyint");

      migrationBuilder.AlterColumn<short>(
          name: "InstallationId",
          table: "ForecastAccuracyRates",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "Temperature",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(byte),
          oldType: "tinyint");

      migrationBuilder.AlterColumn<short>(
          name: "Pressure",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "Pm25",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "Pm10",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "Pm1",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "InstallationId",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "Pm25",
          table: "ArchiveForecasts",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "Pm10",
          table: "ArchiveForecasts",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<short>(
          name: "InstallationId",
          table: "ArchiveForecasts",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<int>(
          name: "Pm25Error",
          table: "ForecastAccuracyRates",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "Pm10Error",
          table: "ForecastAccuracyRates",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<byte>(
          name: "AirlyCaqiError",
          table: "ForecastAccuracyRates",
          type: "tinyint",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "InstallationId",
          table: "ForecastAccuracyRates",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<byte>(
          name: "Temperature",
          table: "ArchiveMeasurements",
          type: "tinyint",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "Pressure",
          table: "ArchiveMeasurements",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "Pm25",
          table: "ArchiveMeasurements",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "Pm10",
          table: "ArchiveMeasurements",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "Pm1",
          table: "ArchiveMeasurements",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "InstallationId",
          table: "ArchiveMeasurements",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "Pm25",
          table: "ArchiveForecasts",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "Pm10",
          table: "ArchiveForecasts",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<int>(
          name: "InstallationId",
          table: "ArchiveForecasts",
          type: "int",
          nullable: false,
          oldClrType: typeof(short));
    }
  }
}
