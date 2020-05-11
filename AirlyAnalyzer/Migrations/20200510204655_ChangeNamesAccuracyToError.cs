using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ChangeNamesAccuracyToError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastAccuracyRates");

            migrationBuilder.CreateTable(
                name: "ForecastErrors",
                columns: table => new
                {
                    InstallationId = table.Column<short>(nullable: false),
                    FromDateTime = table.Column<DateTime>(nullable: false),
                    TillDateTime = table.Column<DateTime>(nullable: false),
                    AirlyCaqiError = table.Column<short>(nullable: false),
                    Pm25Error = table.Column<short>(nullable: false),
                    Pm10Error = table.Column<short>(nullable: false),
                    ForecastRequestDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastErrors", x => new { x.FromDateTime, x.TillDateTime, x.InstallationId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastErrors");

            migrationBuilder.CreateTable(
                name: "ForecastAccuracyRates",
                columns: table => new
                {
                    FromDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TillDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstallationId = table.Column<short>(type: "smallint", nullable: false),
                    AirlyCaqiError = table.Column<short>(type: "smallint", nullable: false),
                    ForecastRequestDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pm10Error = table.Column<short>(type: "smallint", nullable: false),
                    Pm25Error = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastAccuracyRates", x => new { x.FromDateTime, x.TillDateTime, x.InstallationId });
                });
        }
    }
}
