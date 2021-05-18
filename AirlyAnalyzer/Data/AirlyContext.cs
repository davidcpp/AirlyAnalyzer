namespace AirlyAnalyzer.Data
{
  using AirlyAnalyzer.Models;
  using AirlyAnalyzer.Models.Weather;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Microsoft.Extensions.Configuration;

  public class MeasurementConfiguration
      : IEntityTypeConfiguration<AirQualityMeasurement>
  {
    public void Configure(EntityTypeBuilder<AirQualityMeasurement> builder)
    {
      builder.ToTable("Measurements")
          .HasKey(x => new { x.TillDateTime, x.FromDateTime, x.InstallationId, x.Source });

      builder.Property(x => x.FromDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.TillDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.RequestDateTime).HasColumnType("smalldatetime");
    }
  }

  public class ForecastConfiguration
      : IEntityTypeConfiguration<AirQualityForecast>
  {
    public void Configure(EntityTypeBuilder<AirQualityForecast> builder)
    {
      builder.ToTable("Forecasts")
          .HasKey(x => new { x.TillDateTime, x.FromDateTime, x.InstallationId, x.Source });

      builder.Property(x => x.FromDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.TillDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.RequestDateTime).HasColumnType("smalldatetime");

      builder.Property(x => x.Source)
          .HasConversion<string>()
          .IsUnicode(false)
          .HasMaxLength(_maxSourcePropertyLength);
    }
  }

  public class ForecastErrorConfiguration
      : IEntityTypeConfiguration<AirQualityForecastError>
  {
    private readonly byte _maxSourcePropertyLength;

    private readonly byte _maxPeriodPropertyLength;
    private readonly byte _maxClassPropertyLength;

    public ForecastErrorConfiguration(
        byte maxPeriodPropertyLength, byte maxClassPropertyLength, byte maxSourcePropertyLength)
    {
      _maxPeriodPropertyLength = maxPeriodPropertyLength;
      _maxClassPropertyLength = maxClassPropertyLength;
      _maxSourcePropertyLength = maxSourcePropertyLength;
    }

    public void Configure(EntityTypeBuilder<AirQualityForecastError> builder)
    {
      builder.ToTable("ForecastErrors").HasKey(x => new
          { x.Period, x.Class, x.TillDateTime, x.FromDateTime, x.InstallationId, x.Source });

      builder.Property(x => x.FromDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.TillDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.RequestDateTime).HasColumnType("smalldatetime");

      builder.Property(x => x.Period)
          .HasConversion<string>()
          .IsUnicode(false)
          .HasMaxLength(_maxPeriodPropertyLength);

      builder.Property(x => x.Class)
          .HasConversion<string>()
          .IsUnicode(false)
          .HasMaxLength(_maxClassPropertyLength);
    }
  }

  public class InstallationInfoConfiguration
      : IEntityTypeConfiguration<InstallationInfo>
  {
    public void Configure(EntityTypeBuilder<InstallationInfo> builder)
    {
      builder.ToTable("InstallationInfos").HasKey(x => x.InstallationId);
      builder.OwnsOne(i => i.Address);
      builder.Property(x => x.UpdateDate).HasColumnType("date");
    }
  }

  public class WeatherMeasurementConfiguration
    : IEntityTypeConfiguration<WeatherMeasurement>
  {
    public void Configure(EntityTypeBuilder<WeatherMeasurement> builder)
    {
      builder.ToTable("WeatherMeasurements")
          .HasKey(x => new { x.Year, x.Month, x.Day, x.Hour, x.InstallationId });
    }
  }

  public class AirlyContext : DbContext
  {
    private readonly byte _maxPeriodPropertyLength;
    private readonly byte _maxClassPropertyLength;

    public AirlyContext(
        DbContextOptions<AirlyContext> options, IConfiguration config)
        : base(options)
    {
      _maxPeriodPropertyLength =
          config.GetValue<byte>("AppSettings:Databases:MaxPeriodPropertyLength");

      _maxClassPropertyLength = config.GetValue<byte>(
          "AppSettings:Databases:MaxForecastErrorClassPropertyLength");
    }

    public DbSet<AirQualityMeasurement> Measurements { get; set; }

    public DbSet<AirQualityForecast> Forecasts { get; set; }

    public DbSet<AirQualityForecastError> ForecastErrors { get; set; }

    public DbSet<InstallationInfo> InstallationInfos { get; set; }

    public DbSet<WeatherMeasurement> WeatherMeasurements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfiguration(new MeasurementConfiguration());
      modelBuilder.ApplyConfiguration(new ForecastConfiguration());
      modelBuilder.ApplyConfiguration(new ForecastErrorConfiguration(
          _maxPeriodPropertyLength, _maxClassPropertyLength));
      modelBuilder.ApplyConfiguration(new InstallationInfoConfiguration());
      modelBuilder.ApplyConfiguration(new WeatherMeasurementConfiguration());

      modelBuilder.HasAnnotation(
          "SqlServer:ValueGenerationStrategy",
          SqlServerValueGenerationStrategy.None);
    }
  }
}
