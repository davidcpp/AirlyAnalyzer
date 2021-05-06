using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class AddWeatherMeasurementsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherMeasurements",
                columns: table => new
                {
                    Year = table.Column<short>(type: "smallint", nullable: false),
                    Month = table.Column<byte>(type: "tinyint", nullable: false),
                    Day = table.Column<byte>(type: "tinyint", nullable: false),
                    Hour = table.Column<byte>(type: "tinyint", nullable: false),
                    InstallationId = table.Column<int>(type: "int", nullable: false),
                    CloudCover = table.Column<byte>(type: "tinyint", nullable: false),
                    Humidity = table.Column<byte>(type: "tinyint", nullable: false),
                    Pressure = table.Column<float>(type: "real", nullable: false),
                    Rain6h = table.Column<float>(type: "real", nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: false),
                    Visibility = table.Column<int>(type: "int", nullable: false),
                    WindSpeed = table.Column<float>(type: "real", nullable: false),
                    WindDirection = table.Column<short>(type: "smallint", nullable: false),
                    WindGust = table.Column<float>(type: "real", nullable: false),
                    AirlyCaqi = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherMeasurements", x => new { x.Year, x.Month, x.Day, x.Hour, x.InstallationId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherMeasurements");
        }
    }
}
