using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit.Abstractions;

namespace Timetable.Web.IntegrationTest
{
    public class WebServiceFixture
    {
        public static TimeSpan Timeout = new TimeSpan(0, 1, 0);
        
        public IHost Host { get; }

        public WebServiceFixture(IMessageSink logging)
        {
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
    }
}