using System;
using System.Composition;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using Timetable.Web.Plugin;

namespace Timetable.Web
{
    [Export(typeof(IPlugin))]
    public class SwaggerPlugin : IPlugin
    {
        public void ConfigureServices(IServiceCollection services, ILogger logger)
        {
            services.AddSwaggerGen(ConfigureSwagger);
        }

        private void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Title = "Timetable Service",
                Description = "A simple UK rail timetable service.  Look up services and departures and arrivals",
                Contact = new OpenApiContact()
                {
                    Name = "Phil Sharp",
                },
                License = new OpenApiLicense()
                {
                    Name = "MIT",
                    Url = new Uri(@"https://github.com/Phils0/UkTimetableService/blob/master/LICENSE") 
                }           
            });

            var controllerAssembly = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var controllerPath = Path.Combine(AppContext.BaseDirectory, controllerAssembly);
            options.IncludeXmlComments(controllerPath);
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger logger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Timetable Service V1"); });
        }
    }
}