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
    [Migration("20200510204655_ChangeNamesAccuracyToError")]
    partial class ChangeNamesAccuracyToError
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityForecast", b =>
                {
                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("datetime2");

                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<byte>("AirlyCaqi")
                        .HasColumnType("tinyint");

                    b.Property<short>("Pm10")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm25")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("RequestDateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("FromDateTime", "TillDateTime", "InstallationId");

                    b.ToTable("ArchiveForecasts");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityForecastError", b =>
                {
                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("datetime2");

                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<short>("AirlyCaqiError")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("ForecastRequestDateTime")
                        .HasColumnType("datetime2");

                    b.Property<short>("Pm10Error")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm25Error")
                        .HasColumnType("smallint");

                    b.HasKey("FromDateTime", "TillDateTime", "InstallationId");

                    b.ToTable("ForecastErrors");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityMeasurement", b =>
                {
                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("datetime2");

                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<byte>("AirlyCaqi")
                        .HasColumnType("tinyint");

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
                        .HasColumnType("datetime2");

                    b.Property<short>("Temperature")
                        .HasColumnType("smallint");

                    b.HasKey("FromDateTime", "TillDateTime", "InstallationId");

                    b.ToTable("ArchiveMeasurements");
                });
#pragma warning restore 612, 618
        }
    }
}
