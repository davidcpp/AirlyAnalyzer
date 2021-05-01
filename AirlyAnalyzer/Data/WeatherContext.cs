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

    public virtual DbSet<DaneMeteo2020> DaneMeteo2020s { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DaneMeteo2020>(entity =>
      {
        entity.HasKey(e => new { e.Month, e.Day, e.Hour });

        entity.ToTable("Dane_meteo_2020");
      });
    }
  }
}
