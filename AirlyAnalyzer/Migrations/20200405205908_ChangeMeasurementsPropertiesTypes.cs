using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
  public partial class ChangeMeasurementsPropertiesTypes : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<short>(
          name: "Pm25Error",
          table: "ForecastAccuracyRates",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "Pm10Error",
          table: "ForecastAccuracyRates",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "AirlyCaqiError",
          table: "ForecastAccuracyRates",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "Temperature",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "Pressure",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "Pm25",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "Pm10",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "Pm1",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<byte>(
          name: "Humidity",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<byte>(
          name: "AirlyCaqi",
          table: "ArchiveMeasurements",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "Pm25",
          table: "ArchiveForecasts",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<short>(
          name: "Pm10",
          table: "ArchiveForecasts",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");

      migrationBuilder.AlterColumn<byte>(
          name: "AirlyCaqi",
          table: "ArchiveForecasts",
          nullable: false,
          oldClrType: typeof(double),
          oldType: "float");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<double>(
          name: "Pm25Error",
          table: "ForecastAccuracyRates",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "Pm10Error",
          table: "ForecastAccuracyRates",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "AirlyCaqiError",
          table: "ForecastAccuracyRates",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "Temperature",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "Pressure",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "Pm25",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "Pm10",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "Pm1",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "Humidity",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          oldClrType: typeof(byte));

      migrationBuilder.AlterColumn<double>(
          name: "AirlyCaqi",
          table: "ArchiveMeasurements",
          type: "float",
          nullable: false,
          oldClrType: typeof(byte));

      migrationBuilder.AlterColumn<double>(
          name: "Pm25",
          table: "ArchiveForecasts",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "Pm10",
          table: "ArchiveForecasts",
          type: "float",
          nullable: false,
          oldClrType: typeof(short));

      migrationBuilder.AlterColumn<double>(
          name: "AirlyCaqi",
          table: "ArchiveForecasts",
          type: "float",
          nullable: false,
          oldClrType: typeof(byte));
    }
  }
}
