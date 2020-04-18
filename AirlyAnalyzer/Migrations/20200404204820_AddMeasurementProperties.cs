using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
  public partial class AddMeasurementProperties : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<double>(
          name: "Humidity",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pm1",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Pressure",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "Temperature",
          table: "ArchiveMeasurements",
          nullable: false,
          defaultValue: 0.0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "Humidity",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "Pm1",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "Pressure",
          table: "ArchiveMeasurements");

      migrationBuilder.DropColumn(
          name: "Temperature",
          table: "ArchiveMeasurements");
    }
  }
}
