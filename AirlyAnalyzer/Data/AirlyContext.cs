namespace AirlyAnalyzer.Data
{
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;

  public class MeasurementConfiguration
      : IEntityTypeConfiguration<AirQualityMeasurement>
  {
    public void Configure(EntityTypeBuilder<AirQualityMeasurement> builder)
    {
      builder.ToTable("Measurements")
          .HasKey(x => new { x.TillDateTime, x.FromDateTime, x.InstallationId });

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
          .HasKey(x => new { x.TillDateTime, x.FromDateTime, x.InstallationId });

      builder.Property(x => x.FromDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.TillDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.RequestDateTime).HasColumnType("smalldatetime");
    }
  }

  public class ForecastErrorConfiguration
      : IEntityTypeConfiguration<AirQualityForecastError>
  {
    private readonly byte _maxPeriodPropertyLength;

    public ForecastErrorConfiguration(byte maxPeriodPropertyLength)
    {
      _maxPeriodPropertyLength = maxPeriodPropertyLength;
    }

    public void Configure(EntityTypeBuilder<AirQualityForecastError> builder)
    {
      builder.ToTable("ForecastErrors").HasKey(x =>
          new { x.Period, x.TillDateTime, x.FromDateTime, x.InstallationId });

      builder.Property(x => x.Period)
          .HasConversion<string>()
          .IsUnicode(false)
          .HasMaxLength(_maxPeriodPropertyLength);

      builder.Property(x => x.FromDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.TillDateTime).HasColumnType("smalldatetime");
      builder.Property(x => x.RequestDateTime).HasColumnType("smalldatetime");
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

  public class AirlyContext : DbContext
  {
    private readonly byte _maxPeriodPropertyLength;
    public static readonly ILoggerFactory _loggerFactory =
        LoggerFactory.Create(builder => builder.AddDebug());

    public AirlyContext(
        DbContextOptions<AirlyContext> options, IConfiguration config)
        : base(options)
    {
      _maxPeriodPropertyLength =
          config.GetValue<byte>("AppSettings:Databases:MaxPeriodPropertyLength");
    }

    public DbSet<AirQualityMeasurement> Measurements { get; set; }

    public DbSet<AirQualityForecast> Forecasts { get; set; }

    public DbSet<AirQualityForecastError> ForecastErrors { get; set; }

    public DbSet<InstallationInfo> InstallationInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      base.OnConfiguring(optionsBuilder);

      optionsBuilder.EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseLoggerFactory(_loggerFactory);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfiguration(new MeasurementConfiguration());
      modelBuilder.ApplyConfiguration(new ForecastConfiguration());
      modelBuilder.ApplyConfiguration(
          new ForecastErrorConfiguration(_maxPeriodPropertyLength));
      modelBuilder.ApplyConfiguration(new InstallationInfoConfiguration());

      modelBuilder.HasAnnotation(
          "SqlServer:ValueGenerationStrategy",
          SqlServerValueGenerationStrategy.None);
    }
  }
}
