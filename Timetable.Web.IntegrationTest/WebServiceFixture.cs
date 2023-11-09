using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace Timetable.Web.IntegrationTest
{
    public class WebServiceFixture : IDisposable 
    {
        public static TimeSpan Timeout = new TimeSpan(0, 0, 10);
        
        public ILogger Logger { get; }
        public IHost Host { get; private set; }

        public WebServiceFixture(IMessageSink logging)
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(logging, LogEventLevel.Verbose)
                .CreateLogger();
            
            Logger.Debug("Creating Host");
            var hostBuilder = CreateBuilder();
            Host = StartTestServer(hostBuilder);

            IHostBuilder CreateBuilder()
            {
                var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(new string[0])
                    .UseSerilog((context, config) =>
                    {
                        config.WriteTo.TestOutput(logging);
                        config.Enrich.FromLogContext();
                    })
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseTestServer();
                        webBuilder.UseStartup<Startup>();
                    });
                return builder;
            }
            
            IHost StartTestServer(IHostBuilder builder)
            {
                var task = builder.StartAsync();
                var started = task.Wait(Timeout);
                if (!started)
                    throw new Exception("Failed to start Test Server");
                return task.Result;
            }
        }
        
        public bool IsHealthy()
        {
                var client = Host.GetTestClient();
                var responseTask = client.GetAsync(@"/health");
                var got = responseTask.Wait(Timeout);
                return got && responseTask.Result.IsSuccessStatusCode;
        }

        public void Dispose()
        {
            if (Host != null)
            {
                Logger.Debug("Disposing Host");
                var cancellation = new CancellationTokenSource();
                var task = Host.StopAsync(cancellation.Token);
                var shutdown = task.Wait(Timeout);
                if (!shutdown)
                {
                    Logger.Debug("Disposing Host: Cancelling");
                    cancellation.Cancel();
                    shutdown = task.Wait(Timeout);
                }
                if(!shutdown)
                    throw new Exception("Failed to shutdown web host");

                Host = null;
                Logger.Debug("Disposed Host");
            }
        }
    }
}