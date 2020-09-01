using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Timetable.Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                System.Diagnostics.Activity.DefaultIdFormat = ActivityIdFormat.W3C;
                var host = CreateWebHostBuilder(args).Build();
                Log.Information("Built web host");                  
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.Information("Web host terminating");
            }
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog((context, config) =>
                {
                    Logging.Configure(context.Configuration, config);
                });
    }
}
