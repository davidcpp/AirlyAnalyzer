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
                    b.Property<string>("ErrorType")
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

                    b.HasKey("ErrorType", "TillDateTime", "FromDateTime", "InstallationId");

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

                    b.Navigation("Address");
                });
#pragma warning restore 612, 618
        }
    }
}
