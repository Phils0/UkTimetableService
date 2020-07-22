using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using Timetable.Web.Plugin;

namespace Timetable.Web.ServiceConfiguration
{
    internal class Swagger : IPlugin
    {
        public ILogger Logger { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", ApiInfo);

                    var controllerAssembly = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var controllerPath = Path.Combine(AppContext.BaseDirectory, controllerAssembly);
                    options.IncludeXmlComments(controllerPath);
                });
        }

        private OpenApiInfo ApiInfo => new OpenApiInfo()
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
            };

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Timetable Service V1"); });
        }
        
        public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
        {
            // Do nothing
        }
    }
}