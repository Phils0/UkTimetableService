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
using Serilog.Context;

namespace Timetable.Web
{
    public class Startup
    {
        private static readonly TimeSpan Timeout = new TimeSpan(0, 5, 0);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Logger = Log.Logger;
            Plugins = Plugins.FindPlugins(AppContext.BaseDirectory, Logger);
        }

        public Plugins Plugins { get; }
        public IConfiguration Configuration { get; }
        public ILogger Logger { get;  }
        public bool HasLoadedData { get; private set; } = false;
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var factory = new Factory(Factory.MapperConfiguration, Configuration, Logger);
            var data = LoadData(factory);

            services
                .AddSingleton<ILocationData>(data.Locations)
                .AddSingleton<ITimetable>(data.Timetable)
                .AddSingleton<IFilterFactory>(new GatherFilterFactory(Logger))
                .AddSingleton<IMapper>(factory.CreateMapper()) //TODO Swap to scoped
                .AddSingleton<ILogger>(Log.Logger)
                .AddSingleton<Model.Configuration>(ServiceConfigurationFactory.Create(data.Archive))
                .AddControllers();
            services.AddHealthChecks().
                AddCheck("Data", 
                    () => HasLoadedData ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Data not loaded"), 
                    tags: new[] { "data" });
            
            Plugins.ConfigureServices(services);
        }

        private Data LoadData(Factory factory)
        {
            var archive = factory.Archive;
            var activity = new System.Diagnostics.Activity("LoadData").Start();
            using (LogContext.PushProperty("TraceId", activity.TraceId.ToHexString(), true))
            {
                try
                {
                    var loader = factory.CreateDataLoader();
                    var loaderTask = loader.LoadAsync(CancellationToken.None);
                    HasLoadedData = loaderTask.Wait(Timeout);

                    if (!HasLoadedData)
                        throw new InvalidDataException($"Timeout loading file: {archive.FullName}");
                    
                    return loaderTask.Result;
                }
                catch (Exception e)
                {
                    Log.Fatal(e, "Timetable not loaded: {file}", archive.FullName);
                    throw;
                }
                finally
                {
                    activity.Stop();
                    Log.Information("Data loaded in: {duration}ms", activity.Duration.TotalMilliseconds);
                }
            }
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
            
            Plugins.Configure(app, env);

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