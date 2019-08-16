using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
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
                .AddSingleton<IFilterFactory>(new GatherFilterFactory())
                .AddSingleton<IMapper>(factory.CreateMapper()) //TODO Swap to scoped
                .AddSingleton<ILogger>(Log.Logger)
                .AddSwaggerGen(ConfigureSwagger)
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
            options.SwaggerDoc("v1", new Info
            {
                Version = "v1",
                Title = "Timetable Service",
                Description = "A simple UK rail timetable service.  Look up services and departures and arrivals",
                Contact = new Contact()
                {
                    Name = "Phil Sharp",
                },
                License = new License()
                {
                    Name = "MIT",
                    Url = @"https://github.com/Phils0/UkTimetableService/blob/master/LICENSE"
                }           
            });

            var controllerAssembly = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var controllerPath = Path.Combine(AppContext.BaseDirectory, controllerAssembly);
            options.IncludeXmlComments(controllerPath);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseStatusCodePages();
            app.UseHttpsRedirection();
            app.UseMvc();
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