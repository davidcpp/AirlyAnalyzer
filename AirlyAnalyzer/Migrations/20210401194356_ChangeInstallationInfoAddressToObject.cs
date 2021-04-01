using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ChangeInstallationInfoAddressToObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Street",
                table: "InstallationInfos",
                newName: "Address_Street");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "InstallationInfos",
                newName: "Address_Number");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "InstallationInfos",
                newName: "Address_Country");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "InstallationInfos",
                newName: "Address_City");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address_Street",
                table: "InstallationInfos",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "Address_Number",
                table: "InstallationInfos",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "Address_Country",
                table: "InstallationInfos",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "Address_City",
                table: "InstallationInfos",
                newName: "City");
        }
    }
}
