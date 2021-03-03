using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ImplementAirQualityObjectInheritance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pm25PctError",
                table: "ForecastErrors",
                newName: "Pm25Pct");

            migrationBuilder.RenameColumn(
                name: "Pm25Error",
                table: "ForecastErrors",
                newName: "Pm25");

            migrationBuilder.RenameColumn(
                name: "Pm10PctError",
                table: "ForecastErrors",
                newName: "Pm10Pct");

            migrationBuilder.RenameColumn(
                name: "Pm10Error",
                table: "ForecastErrors",
                newName: "Pm10");

            migrationBuilder.RenameColumn(
                name: "AirlyCaqiPctError",
                table: "ForecastErrors",
                newName: "AirlyCaqiPct");

            migrationBuilder.RenameColumn(
                name: "AirlyCaqiError",
                table: "ForecastErrors",
                newName: "AirlyCaqi");

            migrationBuilder.AlterColumn<short>(
                name: "AirlyCaqi",
                table: "ArchiveMeasurements",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<short>(
                name: "AirlyCaqi",
                table: "ArchiveForecasts",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pm25Pct",
                table: "ForecastErrors",
                newName: "Pm25PctError");

            migrationBuilder.RenameColumn(
                name: "Pm25",
                table: "ForecastErrors",
                newName: "Pm25Error");

            migrationBuilder.RenameColumn(
                name: "Pm10Pct",
                table: "ForecastErrors",
                newName: "Pm10PctError");

            migrationBuilder.RenameColumn(
                name: "Pm10",
                table: "ForecastErrors",
                newName: "Pm10Error");

            migrationBuilder.RenameColumn(
                name: "AirlyCaqiPct",
                table: "ForecastErrors",
                newName: "AirlyCaqiPctError");

            migrationBuilder.RenameColumn(
                name: "AirlyCaqi",
                table: "ForecastErrors",
                newName: "AirlyCaqiError");

            migrationBuilder.AlterColumn<byte>(
                name: "AirlyCaqi",
                table: "ArchiveMeasurements",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<byte>(
                name: "AirlyCaqi",
                table: "ArchiveForecasts",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");
        }
    }
}
