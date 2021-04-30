﻿// <auto-generated />
using AirlyAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AirlyAnalyzer.Migrations.Weather
{
    [DbContext(typeof(WeatherContext))]
    [Migration("20210422213052_AddWeatherDatabase")]
    partial class AddWeatherDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None)
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("AirlyAnalyzer.Models.OpenWeatherForecastObject", b =>
                {
                    b.Property<long>("Time")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<float>("AirlyCaqi")
                        .HasColumnType("real");

                    b.Property<byte>("Cloudiness")
                        .HasColumnType("tinyint");

                    b.Property<byte>("Humidity")
                        .HasColumnType("tinyint");

                    b.Property<short>("Pressure")
                        .HasColumnType("smallint");

                    b.Property<float>("PropababilityOfPrecipitation")
                        .HasColumnType("real");

                    b.Property<float>("Temperature")
                        .HasColumnType("real");

                    b.Property<int>("Visibility")
                        .HasColumnType("int");

                    b.Property<short>("WindBearing")
                        .HasColumnType("smallint");

                    b.Property<float>("WindGust")
                        .HasColumnType("real");

                    b.Property<float>("WindSpeed")
                        .HasColumnType("real");

                    b.HasKey("Time");

                    b.ToTable("WeatherMeasurements");
                });

            modelBuilder.Entity("AirlyAnalyzer.Models.OpenWeatherForecastObject", b =>
                {
                    b.OwnsOne("AirlyAnalyzer.Models.OpenWeatherRain", "Rain", b1 =>
                        {
                            b1.Property<long>("OpenWeatherForecastObjectTime")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bigint");

                            b1.Property<float>("OneHour")
                                .HasColumnType("real");

                            b1.Property<float>("ThreeHours")
                                .HasColumnType("real");

                            b1.HasKey("OpenWeatherForecastObjectTime");

                            b1.ToTable("WeatherMeasurements");

                            b1.WithOwner()
                                .HasForeignKey("OpenWeatherForecastObjectTime");
                        });

                    b.Navigation("Rain");
                });
#pragma warning restore 612, 618
        }
    }
}