using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class AddSourcePropertyToAirQualityObject : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Measurements",
                type: "varchar(35)",
                unicode: false,
                maxLength: 35,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Forecasts",
                type: "varchar(35)",
                unicode: false,
                maxLength: 35,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "ForecastErrors",
                type: "varchar(35)",
                unicode: false,
                maxLength: 35,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Measurements",
                table: "Measurements",
                columns: new[] { "TillDateTime", "FromDateTime", "InstallationId", "Source" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Forecasts",
                table: "Forecasts",
                columns: new[] { "TillDateTime", "FromDateTime", "InstallationId", "Source" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "Period", "Class", "TillDateTime", "FromDateTime", "InstallationId", "Source" });
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

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Forecasts");

            migrationBuilder.DropColumn(
                name: "Source",
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
                columns: new[] { "Period", "Class", "TillDateTime", "FromDateTime", "InstallationId" });
        }
    }
}
