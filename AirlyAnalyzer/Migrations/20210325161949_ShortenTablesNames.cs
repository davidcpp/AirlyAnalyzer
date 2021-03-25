using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ShortenTablesNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts");

            migrationBuilder.RenameTable(
                name: "ArchiveMeasurements",
                newName: "Measurements");

            migrationBuilder.RenameTable(
                name: "ArchiveForecasts",
                newName: "Forecasts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Measurements",
                table: "Measurements",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Forecasts",
                table: "Forecasts",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Measurements",
                table: "Measurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Forecasts",
                table: "Forecasts");

            migrationBuilder.RenameTable(
                name: "Measurements",
                newName: "ArchiveMeasurements");

            migrationBuilder.RenameTable(
                name: "Forecasts",
                newName: "ArchiveForecasts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });
        }
    }
}
