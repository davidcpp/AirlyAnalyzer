﻿// <auto-generated />
using System;
using AirlyAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AirlyAnalyzer.Migrations
{
    [DbContext(typeof(AirlyContext))]
    [Migration("20210303175053_ImplementAirQualityObjectInheritance")]
    partial class ImplementAirQualityObjectInheritance
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None)
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityForecast", b =>
                {
                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<short>("AirlyCaqi")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm10")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm25")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("RequestDateTime")
                        .HasColumnType("smalldatetime");

                    b.HasKey("InstallationId", "FromDateTime", "TillDateTime");

                    b.ToTable("ArchiveForecasts");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityForecastError", b =>
                {
                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<short>("AirlyCaqi")
                        .HasColumnType("smallint");

                    b.Property<short>("AirlyCaqiPct")
                        .HasColumnType("smallint");

                    b.Property<string>("ErrorType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .HasColumnType("varchar(20)");

                    b.Property<short>("Pm10")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm10Pct")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm25")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm25Pct")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("RequestDateTime")
                        .HasColumnType("smalldatetime");

                    b.HasKey("InstallationId", "FromDateTime", "TillDateTime");

                    b.ToTable("ForecastErrors");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityMeasurement", b =>
                {
                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<short>("AirlyCaqi")
                        .HasColumnType("smallint");

                    b.Property<byte>("Humidity")
                        .HasColumnType("tinyint");

                    b.Property<short>("Pm1")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm10")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm25")
                        .HasColumnType("smallint");

                    b.Property<short>("Pressure")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("RequestDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<short>("Temperature")
                        .HasColumnType("smallint");

                    b.HasKey("InstallationId", "FromDateTime", "TillDateTime");

                    b.ToTable("ArchiveMeasurements");
                });
#pragma warning restore 612, 618
        }
    }
}
