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
    public DbSet<AirQualityForecastError> ForecastErrors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<AirQualityForecast>()
        .ToTable("ArchiveForecasts")
        .HasKey(x => new { x.InstallationId, x.FromDateTime, x.TillDateTime});
      modelBuilder.Entity<AirQualityMeasurement>()
        .ToTable("ArchiveMeasurements")
        .HasKey(x => new { x.InstallationId, x.FromDateTime, x.TillDateTime });
      modelBuilder.Entity<AirQualityForecastError>()
        .ToTable("ForecastErrors")
        .HasKey(x => new { x.InstallationId, x.FromDateTime, x.TillDateTime });

      modelBuilder
        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);
    }
  }
}
