using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ChangePrimaryKeyPropertiesOrderOfAirQualityObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Measurements",
                table: "Measurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Forecasts",
                table: "Forecasts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Measurements",
                table: "Measurements",
                columns: new[] { "TillDateTime", "FromDateTime", "InstallationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Forecasts",
                table: "Forecasts",
                columns: new[] { "TillDateTime", "FromDateTime", "InstallationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "ErrorType", "TillDateTime", "FromDateTime", "InstallationId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Measurements",
                table: "Measurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Forecasts",
                table: "Forecasts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Measurements",
                table: "Measurements",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Forecasts",
                table: "Forecasts",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime", "ErrorType" });
        }
    }
}
