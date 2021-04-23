using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations.Weather
{
    public partial class ReplaceWeatherMeasurementsClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WeatherMeasurements",
                table: "WeatherMeasurements");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "WeatherMeasurements");

            migrationBuilder.DropColumn(
                name: "Rain_OneHour",
                table: "WeatherMeasurements");

            migrationBuilder.DropColumn(
                name: "Rain_ThreeHours",
                table: "WeatherMeasurements");

            migrationBuilder.RenameColumn(
                name: "WindBearing",
                table: "WeatherMeasurements",
                newName: "WindDirection");

            migrationBuilder.RenameColumn(
                name: "PropababilityOfPrecipitation",
                table: "WeatherMeasurements",
                newName: "Rain6h");

            migrationBuilder.RenameColumn(
                name: "Cloudiness",
                table: "WeatherMeasurements",
                newName: "CloudCover");

            migrationBuilder.AddColumn<byte>(
                name: "Month",
                table: "WeatherMeasurements",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Day",
                table: "WeatherMeasurements",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Hour",
                table: "WeatherMeasurements",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeatherMeasurements",
                table: "WeatherMeasurements",
                columns: new[] { "Month", "Day", "Hour" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WeatherMeasurements",
                table: "WeatherMeasurements");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "WeatherMeasurements");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "WeatherMeasurements");

            migrationBuilder.DropColumn(
                name: "Hour",
                table: "WeatherMeasurements");

            migrationBuilder.RenameColumn(
                name: "WindDirection",
                table: "WeatherMeasurements",
                newName: "WindBearing");

            migrationBuilder.RenameColumn(
                name: "Rain6h",
                table: "WeatherMeasurements",
                newName: "PropababilityOfPrecipitation");

            migrationBuilder.RenameColumn(
                name: "CloudCover",
                table: "WeatherMeasurements",
                newName: "Cloudiness");

            migrationBuilder.AddColumn<long>(
                name: "Time",
                table: "WeatherMeasurements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<float>(
                name: "Rain_OneHour",
                table: "WeatherMeasurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Rain_ThreeHours",
                table: "WeatherMeasurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeatherMeasurements",
                table: "WeatherMeasurements",
                column: "Time");
        }
    }
}
