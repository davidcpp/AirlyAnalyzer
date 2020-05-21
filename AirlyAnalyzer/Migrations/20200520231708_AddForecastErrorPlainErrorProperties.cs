using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class AddForecastErrorPlainErrorProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "AirlyCaqiError",
                table: "ForecastErrors",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Pm10Error",
                table: "ForecastErrors",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Pm25Error",
                table: "ForecastErrors",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirlyCaqiError",
                table: "ForecastErrors");

            migrationBuilder.DropColumn(
                name: "Pm10Error",
                table: "ForecastErrors");

            migrationBuilder.DropColumn(
                name: "Pm25Error",
                table: "ForecastErrors");
        }
    }
}
