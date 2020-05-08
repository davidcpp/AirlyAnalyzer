using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ModifyPrimaryKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastAccuracyRates",
                table: "ForecastAccuracyRates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastAccuracyRates",
                table: "ForecastAccuracyRates",
                columns: new[] { "FromDateTime", "TillDateTime", "InstallationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements",
                columns: new[] { "FromDateTime", "TillDateTime", "InstallationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts",
                columns: new[] { "FromDateTime", "TillDateTime", "InstallationId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastAccuracyRates",
                table: "ForecastAccuracyRates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastAccuracyRates",
                table: "ForecastAccuracyRates",
                columns: new[] { "FromDateTime", "InstallationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements",
                columns: new[] { "FromDateTime", "InstallationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts",
                columns: new[] { "FromDateTime", "InstallationId" });
        }
    }
}
