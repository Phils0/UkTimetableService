using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using Timetable.Web.Model;
using ILogger = Serilog.ILogger;

namespace Timetable.Web
{
    public class Startup
    {
        private static readonly TimeSpan Timeout = new TimeSpan(0, 5, 0);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var factory = new Factory(Factory.MapperConfiguration, Configuration, Log.Logger);
            var data = LoadData(factory);

            services
                .AddSingleton<ILocationData>(data.Locations)
                .AddSingleton<ITimetable>(data.Timetable)
                .AddSingleton<IFilterFactory>(new GatherFilterFactory(Log.Logger))
                .AddSingleton<IMapper>(factory.CreateMapper()) //TODO Swap to scoped
                .AddSingleton<ILogger>(Log.Logger)
                .AddSingleton<Model.Configuration>(CreateConfiguration())
                .AddSwaggerGen(ConfigureSwagger)
                .AddControllers();
            services.AddHealthChecks();

            Configuration CreateConfiguration()
            {
                return new Model.Configuration()
                {
                    Version = GetType().Assembly.GetName().Version.ToString(),
                    Data = data.Archive
                };
            }
        }

        private static Data LoadData(Factory factory)
        {
            var archive = factory.Archive;

            try
            {
                var loader = factory.CreateDataLoader();
                var loaderTask = loader.LoadAsync(CancellationToken.None);

                var loaded = Task.WaitAll(new[]
                {
                    loaderTask
                }, Timeout);

                if (!loaded)
                    throw new InvalidDataException($"Timeout loading file: {archive.FullName}");

                return loaderTask.Result;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Timetable not loaded: {file}", archive.FullName);
                throw;
            }
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(AddExceptionHandler);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Timetable Service V1"); });
            
            app.UseStatusCodePages();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions() { });
                endpoints.MapControllers();
            });
        }

        private void AddExceptionHandler(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();
                var path = exceptionHandlerPathFeature?.Path;
                var error = exceptionHandlerPathFeature?.Error;
                Log.Error(error, "Unhandled error: {path}", path);

                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
            });
        }
    }
}