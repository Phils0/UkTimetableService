using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Timetable.Web.ServiceConfiguration;

namespace Timetable.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Plugins = ConfigurationFinder.Find(configuration);
        }
        
        internal ServiceConfigurations Plugins { get; }
        
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
            
            app.UseSerilogRequestLogging();
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