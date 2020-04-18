using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
  public partial class InitialCreate : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "ArchiveForecasts",
          columns: table => new
          {
            FromDateTime = table.Column<DateTime>(nullable: false),
            InstallationId = table.Column<int>(nullable: false),
            Pm25Value = table.Column<double>(nullable: false),
            Pm10Value = table.Column<double>(nullable: false),
            AirlyCaqiValue = table.Column<double>(nullable: false),
            TillDateTime = table.Column<DateTime>(nullable: false),
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ArchiveForecasts", x => x.FromDateTime);
          });

      migrationBuilder.CreateTable(
          name: "ArchiveMeasurements",
          columns: table => new
          {
            FromDateTime = table.Column<DateTime>(nullable: false),
            InstallationId = table.Column<int>(nullable: false),
            Pm25Value = table.Column<double>(nullable: false),
            Pm10Value = table.Column<double>(nullable: false),
            AirlyCaqiValue = table.Column<double>(nullable: false),
            TillDateTime = table.Column<DateTime>(nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ArchiveMeasurements", x => x.FromDateTime);
          });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "ArchiveForecasts");

      migrationBuilder.DropTable(
          name: "ArchiveMeasurements");
    }
  }
}
