using AirlyAnalyzer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AirlyAnalyzer.Data
{
  public class AirlyContext : DbContext
  {
    public AirlyContext(DbContextOptions<AirlyContext> options) : base(options)
    {
    }

    public DbSet<AirQualityForecast> ArchiveForecasts { get; set; }
    public DbSet<AirQualityMeasurement> ArchiveMeasurements { get; set; }
    public DbSet<AirQualityForecastAccuracy> ForecastAccuracyRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<AirQualityForecast>()
        .ToTable("ArchiveForecasts")
        .HasKey(x => new { x.FromDateTime, x.TillDateTime, x.InstallationId });
      modelBuilder.Entity<AirQualityMeasurement>()
        .ToTable("ArchiveMeasurements")
        .HasKey(x => new { x.FromDateTime, x.TillDateTime, x.InstallationId });
      modelBuilder.Entity<AirQualityForecastAccuracy>()
        .ToTable("ForecastAccuracyRates")
        .HasKey(x => new { x.FromDateTime, x.TillDateTime, x.InstallationId });

      modelBuilder
        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);
    }
  }
}
