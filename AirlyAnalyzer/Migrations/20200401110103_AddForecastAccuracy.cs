using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
  public partial class AddForecastAccuracy : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "ForecastAccuracyRates",
          columns: table => new
          {
            InstallationId = table.Column<int>(nullable: false),
            FromDateTime = table.Column<DateTime>(nullable: false),
            Pm25Accuracy = table.Column<double>(nullable: false),
            Pm10Accuracy = table.Column<double>(nullable: false),
            AirlyCaqiAccuracy = table.Column<double>(nullable: false),
            ForecastRequestDate = table.Column<DateTime>(nullable: false),
            TillDateTime = table.Column<DateTime>(nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ForecastAccuracyRates", x => new { x.FromDateTime, x.InstallationId });
          });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "ForecastAccuracyRates");
    }
  }
}
