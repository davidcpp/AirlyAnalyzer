using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class AddInstallationAddressPropertyToAirQualityObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstallationAddress",
                table: "Measurements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstallationAddress",
                table: "Forecasts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstallationAddress",
                table: "ForecastErrors",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstallationAddress",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "InstallationAddress",
                table: "Forecasts");

            migrationBuilder.DropColumn(
                name: "InstallationAddress",
                table: "ForecastErrors");
        }
    }
}
