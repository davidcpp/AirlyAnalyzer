﻿// <auto-generated />
using System;
using AirlyAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AirlyAnalyzer.Migrations
{
    [DbContext(typeof(AirlyContext))]
    partial class AirlyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None)
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("AirlyAnalyzer.InstallationInfo", b =>
                {
                    b.Property<short>("InstallationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("date");

                    b.HasKey("InstallationId");

                    b.ToTable("InstallationInfos");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityForecast", b =>
                {
                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<short>("AirlyCaqi")
                        .HasColumnType("smallint");

                    b.Property<string>("InstallationAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<short>("Pm10")
                        .HasColumnType("smallint");

                    b.Property<short>("Pm25")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("RequestDateTime")
                        .HasColumnType("smalldatetime");

                    b.HasKey("TillDateTime", "FromDateTime", "InstallationId");

                    b.ToTable("Forecasts");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityForecastError", b =>
                {
                    b.Property<string>("Period")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("Class")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .HasColumnType("varchar(20)");

                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<short>("AirlyCaqi")
                        .HasColumnType("smallint");

                    b.Property<short>("AirlyCaqiPct")
                        .HasColumnType("smallint");

                    b.Property<string>("InstallationAddress")
                        .HasColumnType("nvarchar(max)");

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

                    b.HasKey("Period", "Class", "TillDateTime", "FromDateTime", "InstallationId");

                    b.ToTable("ForecastErrors");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.AirQualityMeasurement", b =>
                {
                    b.Property<DateTime>("TillDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<DateTime>("FromDateTime")
                        .HasColumnType("smalldatetime");

                    b.Property<short>("InstallationId")
                        .HasColumnType("smallint");

                    b.Property<short>("AirlyCaqi")
                        .HasColumnType("smallint");

                    b.Property<byte>("Humidity")
                        .HasColumnType("tinyint");

                    b.Property<string>("InstallationAddress")
                        .HasColumnType("nvarchar(max)");

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

                    b.Property<short>("WindBearing")
                        .HasColumnType("smallint");

                    b.Property<float>("WindSpeed")
                        .HasColumnType("real");

                    b.HasKey("TillDateTime", "FromDateTime", "InstallationId");

                    b.ToTable("Measurements");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.Weather.WeatherMeasurement", b =>
                {
                    b.Property<short>("Year")
                        .HasColumnType("smallint");

                    b.Property<byte>("Month")
                        .HasColumnType("tinyint");

                    b.Property<byte>("Day")
                        .HasColumnType("tinyint");

                    b.Property<byte>("Hour")
                        .HasColumnType("tinyint");

                    b.Property<int>("InstallationId")
                        .HasColumnType("int");

                    b.Property<short>("AirlyCaqi")
                        .HasColumnType("smallint");

                    b.Property<byte>("CloudCover")
                        .HasColumnType("tinyint");

                    b.Property<byte>("Humidity")
                        .HasColumnType("tinyint");

                    b.Property<float>("Pressure")
                        .HasColumnType("real");

                    b.Property<float>("Rain6h")
                        .HasColumnType("real");

                    b.Property<float>("Temperature")
                        .HasColumnType("real");

                    b.Property<int>("Visibility")
                        .HasColumnType("int");

                    b.Property<short>("WindDirection")
                        .HasColumnType("smallint");

                    b.Property<float>("WindGust")
                        .HasColumnType("real");

                    b.Property<float>("WindSpeed")
                        .HasColumnType("real");

                    b.HasKey("Year", "Month", "Day", "Hour", "InstallationId");

                    b.ToTable("WeatherMeasurements");
                });

            modelBuilder.Entity("AirlyAnalyzer.InstallationInfo", b =>
                {
                    b.OwnsOne("AirlyAnalyzer.Models.Address", "Address", b1 =>
                        {
                            b1.Property<short>("InstallationInfoInstallationId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("smallint");

                            b1.Property<string>("City")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Country")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Number")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Street")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("InstallationInfoInstallationId");

                            b1.ToTable("InstallationInfos");

                            b1.WithOwner()
                                .HasForeignKey("InstallationInfoInstallationId");
                        });

                    b.OwnsOne("AirlyAnalyzer.Models.Coordinates", "Location", b1 =>
                        {
                            b1.Property<short>("InstallationInfoInstallationId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("smallint");

                            b1.Property<float>("Latitude")
                                .HasColumnType("real");

                            b1.Property<float>("Longitude")
                                .HasColumnType("real");

                            b1.HasKey("InstallationInfoInstallationId");

                            b1.ToTable("InstallationInfos");

                            b1.WithOwner()
                                .HasForeignKey("InstallationInfoInstallationId");
                        });

                    b.Navigation("Address");

                    b.Navigation("Location");
                });
#pragma warning restore 612, 618
        }
    }
}
