namespace AirlyAnalyzer.Data
{
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;

  public class AirlyContext : DbContext
  {
    private readonly byte _maxErrorTypeLength;
    public static readonly ILoggerFactory _loggerFactory =
      LoggerFactory.Create(builder => builder.AddDebug());

    public AirlyContext(DbContextOptions<AirlyContext> options, IConfiguration config) : base(options)
    {
      _maxErrorTypeLength = config.GetValue<byte>("AppSettings:Databases:MaxErrorTypeLength");
    }

    public DbSet<AirQualityForecast> ArchiveForecasts { get; set; }
    public DbSet<AirQualityMeasurement> ArchiveMeasurements { get; set; }
    public DbSet<AirQualityForecastError> ForecastErrors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      base.OnConfiguring(optionsBuilder);

      optionsBuilder.EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseLoggerFactory(_loggerFactory);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<AirQualityForecast>()
        .ToTable("ArchiveForecasts")
        .HasKey(x => new { x.InstallationId, x.FromDateTime, x.TillDateTime });

      modelBuilder.Entity<AirQualityMeasurement>()
        .ToTable("ArchiveMeasurements")
        .HasKey(x => new { x.InstallationId, x.FromDateTime, x.TillDateTime });

      modelBuilder.Entity<AirQualityForecastError>()
        .ToTable("ForecastErrors")
        .HasKey(x => new { x.InstallationId, x.FromDateTime, x.TillDateTime });

      modelBuilder.Entity<AirQualityForecastError>()
        .Property(x => x.ErrorType)
        .HasConversion<string>()
        .IsUnicode(false)
        .HasMaxLength(_maxErrorTypeLength);

      modelBuilder.Entity<AirQualityForecast>()
        .Property(x => x.FromDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder.Entity<AirQualityForecast>()
        .Property(x => x.TillDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder.Entity<AirQualityForecast>()
        .Property(x => x.RequestDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder.Entity<AirQualityMeasurement>()
        .Property(x => x.FromDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder.Entity<AirQualityMeasurement>()
        .Property(x => x.TillDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder.Entity<AirQualityMeasurement>()
        .Property(x => x.RequestDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder.Entity<AirQualityForecastError>()
        .Property(x => x.FromDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder.Entity<AirQualityForecastError>()
        .Property(x => x.TillDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder.Entity<AirQualityForecastError>()
        .Property(x => x.RequestDateTime)
        .HasColumnType("smalldatetime");

      modelBuilder
        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);
    }
  }
}
