namespace AirlyAnalyzer.Data
{
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;

  public class WeatherMeasurementConfiguration
      : IEntityTypeConfiguration<OpenWeatherForecastObject>
  {
    public void Configure(EntityTypeBuilder<OpenWeatherForecastObject> builder)
    {
      builder.ToTable("WeatherMeasurements").HasKey(x => x.Time);
      builder.OwnsOne(i => i.Rain);
    }
  }

  public class WeatherContext : DbContext
  {
    public WeatherContext(DbContextOptions<WeatherContext> options)
        : base(options)
    {
    }

    public DbSet<OpenWeatherForecastObject> WeatherMeasurements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfiguration(new WeatherMeasurementConfiguration());

      modelBuilder.HasAnnotation(
          "SqlServer:ValueGenerationStrategy",
          SqlServerValueGenerationStrategy.None);
    }
  }
}
