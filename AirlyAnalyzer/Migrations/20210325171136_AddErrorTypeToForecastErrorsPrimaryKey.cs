using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class AddErrorTypeToForecastErrorsPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime", "ErrorType" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });
        }
    }
}
