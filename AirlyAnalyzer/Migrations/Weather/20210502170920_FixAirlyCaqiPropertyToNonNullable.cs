using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations.Weather
{
    public partial class FixAirlyCaqiPropertyToNonNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "AirlyCaqi",
                table: "WeatherMeasurements",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "AirlyCaqi",
                table: "WeatherMeasurements",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");
        }
    }
}
