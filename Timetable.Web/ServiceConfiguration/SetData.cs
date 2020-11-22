using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using Timetable.DataLoader;
using Timetable.Web.Plugin;

namespace Timetable.Web.ServiceConfiguration
{
    internal class SetData : IPlugin
    {
        
        private readonly IDataLoader _loader;

        internal SetData(IDataLoader loader, ILogger logger)
        {
            _loader = loader;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public bool HasLoadedData { get; private set; } = false;
        
        public void ConfigureServices(IServiceCollection services)
        {
            var data = Load();

            services
                .AddSingleton<ILocationData>(data.Locations)
                .AddSingleton<ITimetable>(data.Timetable)
                .AddSingleton<ITocLookup>(data.Tocs)
                .AddSingleton<Model.Configuration>(Factory.CreateConfigurationResponse(data.Archive));
        }

        private Data Load()
        {
            var activity = new System.Diagnostics.Activity("LoadData").Start();
            using (LogContext.PushProperty("TraceId", activity.TraceId.ToHexString(), true))
            {
                try
                {
                    var loaderTask = _loader.LoadAsync(CancellationToken.None);
                    HasLoadedData = loaderTask.Wait(Loaders.DataLoader.Timeout);

                    if (!HasLoadedData)
                        throw new InvalidDataException($"Timeout loading data");

                    var data = loaderTask.Result;
                    if(!data.IsLoaded)
                        throw new InvalidDataException($"Data not loaded");
                    
                    return data;
                }
                catch (Exception e)
                {
                    Logger.Fatal(e, "Timetable not loaded");
                    throw;
                }
                finally
                {
                    activity.Stop();
                    Logger.Information("Data loaded in: {duration}ms", activity.Duration.TotalMilliseconds);
                }
            }
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Do nothing
        }

        public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
        {
            // Do nothing
        }
    }
}