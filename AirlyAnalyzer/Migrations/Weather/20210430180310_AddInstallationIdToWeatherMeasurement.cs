using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations.Weather
{
    public partial class AddInstallationIdToWeatherMeasurement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstallationId",
                table: "WeatherMeasurements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstallationId",
                table: "WeatherMeasurements");
        }
    }
}
