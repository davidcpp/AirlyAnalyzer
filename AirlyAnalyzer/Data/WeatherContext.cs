﻿namespace AirlyAnalyzer.Data
{
  using AirlyAnalyzer.Models.Weather;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;

  public class WeatherMeasurementConfiguration
      : IEntityTypeConfiguration<WeatherMeasurement>
  {
    public void Configure(EntityTypeBuilder<WeatherMeasurement> builder)
    {
      builder.ToTable("Dane_meteo_2020").HasKey(x => new { x.Month, x.Day, x.Hour });
    }
  }

  public class WeatherContext : DbContext
  {
    public WeatherContext(DbContextOptions<WeatherContext> options)
        : base(options)
    {
    }

    public virtual DbSet<WeatherMeasurement> WeatherMeasurements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfiguration(new WeatherMeasurementConfiguration());

      modelBuilder.HasAnnotation(
          "SqlServer:ValueGenerationStrategy",
          SqlServerValueGenerationStrategy.None);
    }
  }
}
