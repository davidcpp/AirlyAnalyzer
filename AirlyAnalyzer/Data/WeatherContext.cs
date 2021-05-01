namespace AirlyAnalyzer.Data
{
  using AirlyAnalyzer.Models.Weather;
  using Microsoft.EntityFrameworkCore;

  public class WeatherContext : DbContext
  {
    public WeatherContext()
    {
    }

    public WeatherContext(DbContextOptions<WeatherContext> options)
        : base(options)
    {
    }

    public virtual DbSet<WeatherMeasurement> DaneMeteo2020s { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<WeatherMeasurement>(entity =>
      {
        entity.HasKey(e => new { e.Month, e.Day, e.Hour });

        entity.ToTable("Dane_meteo_2020");
      });
    }
  }
}
