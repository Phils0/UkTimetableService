using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Serilog;
using Timetable.Web.Plugin;

namespace Timetable.Web.ServiceConfiguration
{
    internal class Prometheus : IPlugin
    {
        public ILogger Logger { get; set; }
        public void ConfigureServices(IServiceCollection services)
        {
            // Do nothing
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpMetrics();
        }

        public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapMetrics();
        }
    }
}