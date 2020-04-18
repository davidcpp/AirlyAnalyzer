using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
  public partial class ModifyMeasurementsPrimaryKeys : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropPrimaryKey(
          name: "PK_ArchiveMeasurements",
          table: "ArchiveMeasurements");

      migrationBuilder.DropPrimaryKey(
          name: "PK_ArchiveForecasts",
          table: "ArchiveForecasts");

      migrationBuilder.AddPrimaryKey(
          name: "PK_ArchiveMeasurements",
          table: "ArchiveMeasurements",
          columns: new[] { "FromDateTime", "InstallationId" });

      migrationBuilder.AddPrimaryKey(
          name: "PK_ArchiveForecasts",
          table: "ArchiveForecasts",
          columns: new[] { "FromDateTime", "InstallationId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropPrimaryKey(
          name: "PK_ArchiveMeasurements",
          table: "ArchiveMeasurements");

      migrationBuilder.DropPrimaryKey(
          name: "PK_ArchiveForecasts",
          table: "ArchiveForecasts");

      migrationBuilder.AddPrimaryKey(
          name: "PK_ArchiveMeasurements",
          table: "ArchiveMeasurements",
          column: "FromDateTime");

      migrationBuilder.AddPrimaryKey(
          name: "PK_ArchiveForecasts",
          table: "ArchiveForecasts",
          column: "FromDateTime");
    }
  }
}
