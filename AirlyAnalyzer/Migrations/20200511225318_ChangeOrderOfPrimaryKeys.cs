using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ChangeOrderOfPrimaryKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
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
    }
}
