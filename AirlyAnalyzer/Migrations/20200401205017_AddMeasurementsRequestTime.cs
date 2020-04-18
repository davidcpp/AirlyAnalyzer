using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
  public partial class AddMeasurementsRequestTime : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<DateTime>(
          name: "RequestTime",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

      migrationBuilder.AddColumn<DateTime>(
          name: "RequestTime",
          table: "ArchiveForecasts",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "RequestTime",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "RequestTime",
          table: "ArchiveForecasts");
    }
  }
}
