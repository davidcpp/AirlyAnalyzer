using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlyAnalyzer.Migrations
{
    public partial class ReduceSizeOfDateTimeProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDateTime",
                table: "ForecastErrors",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TillDateTime",
                table: "ForecastErrors",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromDateTime",
                table: "ForecastErrors",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDateTime",
                table: "ArchiveMeasurements",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TillDateTime",
                table: "ArchiveMeasurements",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromDateTime",
                table: "ArchiveMeasurements",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDateTime",
                table: "ArchiveForecasts",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TillDateTime",
                table: "ArchiveForecasts",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromDateTime",
                table: "ArchiveForecasts",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TillDateTime",
                table: "ForecastErrors",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDateTime",
                table: "ForecastErrors",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromDateTime",
                table: "ForecastErrors",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TillDateTime",
                table: "ArchiveMeasurements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDateTime",
                table: "ArchiveMeasurements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromDateTime",
                table: "ArchiveMeasurements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TillDateTime",
                table: "ArchiveForecasts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDateTime",
                table: "ArchiveForecasts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromDateTime",
                table: "ArchiveForecasts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastErrors",
                table: "ForecastErrors",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveMeasurements",
                table: "ArchiveMeasurements",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveForecasts",
                table: "ArchiveForecasts",
                columns: new[] { "InstallationId", "FromDateTime", "TillDateTime" });
        }
    }
}
