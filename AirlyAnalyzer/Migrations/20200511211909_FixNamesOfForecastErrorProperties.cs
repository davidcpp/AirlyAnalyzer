using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class FixNamesOfForecastErrorProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirlyCaqiError",
                table: "ForecastErrors");

            migrationBuilder.DropColumn(
                name: "ForecastRequestDateTime",
                table: "ForecastErrors");

            migrationBuilder.DropColumn(
                name: "Pm10Error",
                table: "ForecastErrors");

            migrationBuilder.DropColumn(
                name: "Pm25Error",
                table: "ForecastErrors");

            migrationBuilder.AddColumn<short>(
                name: "AirlyCaqiPctError",
                table: "ForecastErrors",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Pm10PctError",
                table: "ForecastErrors",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Pm25PctError",
                table: "ForecastErrors",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDateTime",
                table: "ForecastErrors",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirlyCaqiPctError",
                table: "ForecastErrors");

            migrationBuilder.DropColumn(
                name: "Pm10PctError",
                table: "ForecastErrors");

            migrationBuilder.DropColumn(
                name: "Pm25PctError",
                table: "ForecastErrors");

            migrationBuilder.DropColumn(
                name: "RequestDateTime",
                table: "ForecastErrors");

            migrationBuilder.AddColumn<short>(
                name: "AirlyCaqiError",
                table: "ForecastErrors",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ForecastRequestDateTime",
                table: "ForecastErrors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<short>(
                name: "Pm10Error",
                table: "ForecastErrors",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Pm25Error",
                table: "ForecastErrors",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
