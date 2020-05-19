using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Timetable.Web.Plugin
{
    public interface IPlugin
    {
        public void ConfigureServices(IServiceCollection services, ILogger logger);
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger logger);
    }
}