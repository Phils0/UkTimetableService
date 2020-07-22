using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Serilog;
using Timetable.Web.Plugin;

namespace Timetable.Web.ServiceConfiguration
{
    /// <summary>
    /// Plugin must be run after DataPlugin has run
    /// </summary>
    internal class HealthCheck : IPlugin
    {
        private readonly SetData _data;
        private readonly bool _enablePrometheus;

        internal HealthCheck(SetData data, bool enablePrometheus)
        {
            _data = data;
            _enablePrometheus = enablePrometheus;
        }
        
        public ILogger Logger { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddHealthChecks().
                AddCheck("Data", 
                    () => _data.HasLoadedData ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Data not loaded"), 
                    tags: new[] { "data" });

            if (_enablePrometheus)
                builder.ForwardToPrometheus();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Do nothing
        }

        public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions() { });
        }
    }
}