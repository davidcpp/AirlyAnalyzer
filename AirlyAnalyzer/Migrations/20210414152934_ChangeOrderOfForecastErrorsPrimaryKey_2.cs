using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ChangeOrderOfForecastErrorsPrimaryKey_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "Period", "Class", "TillDateTime", "FromDateTime", "InstallationId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "Period", "TillDateTime", "Class", "FromDateTime", "InstallationId" });
        }
    }
}
