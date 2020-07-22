using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Timetable.Web.Plugin
{
    public interface IPlugin
    {
        public ILogger Logger { get; set; }
        public void ConfigureServices(IServiceCollection services);
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env);
        public void ConfigureEndpoints(IEndpointRouteBuilder endpoints);
    }
}