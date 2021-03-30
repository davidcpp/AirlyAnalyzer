namespace AirlyAnalyzer
{
  using System;
  using AirlyAnalyzer.Data;
  using AirlyAnalyzer.Services;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;

  public class Program
  {
    public static void Main(string[] args)
    {
      var host = CreateHostBuilder(args).Build();

      using (var scope = host.Services.CreateScope())
      {
        var services = scope.ServiceProvider;
        try
        {
          var context = services.GetRequiredService<AirlyContext>();
          context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
          var logger = services.GetRequiredService<ILogger<Program>>();
          logger.LogError(ex, "An error occurred creating the DB.");
        }
      }

      host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder
                => webBuilder.UseStartup<Startup>())
            .ConfigureServices(services
                => services.AddHostedService<ProgramController>(x =>
                    new ProgramController(
                        x.GetRequiredService<IServiceProvider>(),
                        x.GetRequiredService<IConfiguration>(),
                        x.GetRequiredService<ILogger<ProgramController>>())));
  }
}
