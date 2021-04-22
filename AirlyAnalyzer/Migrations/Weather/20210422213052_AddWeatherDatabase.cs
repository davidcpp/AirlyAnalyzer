using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations.Weather
{
    public partial class AddWeatherDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherMeasurements",
                columns: table => new
                {
                    Time = table.Column<long>(type: "bigint", nullable: false),
                    Cloudiness = table.Column<byte>(type: "tinyint", nullable: false),
                    Humidity = table.Column<byte>(type: "tinyint", nullable: false),
                    Pressure = table.Column<short>(type: "smallint", nullable: false),
                    PropababilityOfPrecipitation = table.Column<float>(type: "real", nullable: false),
                    Rain_OneHour = table.Column<float>(type: "real", nullable: true),
                    Rain_ThreeHours = table.Column<float>(type: "real", nullable: true),
                    Temperature = table.Column<float>(type: "real", nullable: false),
                    Visibility = table.Column<int>(type: "int", nullable: false),
                    WindSpeed = table.Column<float>(type: "real", nullable: false),
                    WindBearing = table.Column<short>(type: "smallint", nullable: false),
                    WindGust = table.Column<float>(type: "real", nullable: false),
                    AirlyCaqi = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherMeasurements", x => x.Time);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherMeasurements");
        }
    }
}
