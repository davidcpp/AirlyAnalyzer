namespace AirlyAnalyzer
{
  using System;
  using System.IO;
  using System.Text.Json.Serialization;
  using AirlyAnalyzer.Calculation;
  using AirlyAnalyzer.Client;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Services;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;

  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<CookiePolicyOptions>(options =>
      {
        options.CheckConsentNeeded = _ => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
      });

      string dataDirectoryPath
          = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");

      services.AddDbContext<AirlyContext>(options
          => options.UseSqlServer(
              Configuration
                  .GetConnectionString("AirlyDbConnection")
                  .Replace("[DataDirectory]", dataDirectoryPath)));

      services.AddScoped<UnitOfWork>();

      services.AddSingleton<IAirlyApiDownloader>(
          x => new AirlyApiDownloader(
              x.GetRequiredService<IConfiguration>(),
              new WebClientAdapter(new System.Net.WebClient()),
              x.GetRequiredService<ILogger<AirlyApiDownloader>>()));

      services.AddSingleton<IOpenWeatherApiDownloader>(
          x => new OpenWeatherApiDownloader(
              x.GetRequiredService<IConfiguration>(),
              new WebClientAdapter(new System.Net.WebClient()),
              x.GetRequiredService<ILogger<OpenWeatherApiDownloader>>()));

      services.AddSingleton
          <IForecastErrorsCalculator, PlainForecastErrorsCalculator>();
      services.AddSingleton
          <IForecastErrorsCalculator, ScaleForecastErrorsCalculator>();

      services.AddHostedService(x =>
          new ForecastErrorsService(
              x.GetRequiredService<IServiceProvider>(),
              x.GetRequiredService<IConfiguration>(),
              x.GetRequiredService<ILogger<ForecastErrorsService>>()));

      services.AddHostedService(x =>
          new ForecastService(
              x.GetRequiredService<IServiceProvider>(),
              x.GetRequiredService<IConfiguration>(),
              x.GetRequiredService<ILogger<ForecastService>>()));

      services
          .AddControllersWithViews()
          .AddJsonOptions(options =>
              options.JsonSerializerOptions.Converters.Add(
                  new JsonStringEnumConverter()));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=ForecastErrors}/{action=Index}/{id?}");
      });
    }
  }
}
