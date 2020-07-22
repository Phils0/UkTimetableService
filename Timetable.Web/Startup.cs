using System;
using System.IO;
using System.Net;
using System.Threading;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Timetable.Web.Plugin;
using Timetable.Web.ServiceConfiguration;

namespace Timetable.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = new Configuration(configuration);
            Plugins = ConfigurationFinder.Find(Configuration);
        }

        internal ServiceConfigurations Plugins { get; }
        internal Configuration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Plugins.ConfigureServices(services);
            services.AddControllers();
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            Plugins.Configure(app, env);
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                Plugins.ConfigureEndpoints(endpoints);
            });
        }
    }
}