using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Timetable.Web.Plugin;

namespace Timetable.Web.ServiceConfiguration
{
    internal class ServiceConfigurations : List<IPlugin>, IPlugin
    {
        internal ServiceConfigurations(ILogger logger)  
        {
            Logger = logger;
        }
        
        internal ServiceConfigurations(IEnumerable<IPlugin> configurations, ILogger logger) : base(configurations) 
        {
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public new void Add(IPlugin item) 
        {
            base.Add(item);
            item.Logger = Logger;
        }

        public new void AddRange(IEnumerable<IPlugin> items) 
        {
            base.AddRange(items);
            foreach (var item in items)
            {
                item.Logger = Logger;
            }
        }
        
        public void ConfigureServices(IServiceCollection services) => ForEach(configuration =>
                {
                    try
                    {
                        configuration.ConfigureServices(services);
                    }
                    catch (Exception e)
                    {
                        Logger.Warning(e, "Configure Services failed for plugin {configuration}", configuration);
                    }                
                });

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) => ForEach(configuration =>
        {
            try
            {
                configuration.Configure(app, env);
            }
            catch (Exception e)
            {
                Logger.Warning(e, "Configure failed for plugin {configuration}", configuration);
            }               
        });

        public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)=> ForEach(configuration =>
        {
            try
            {
                configuration.ConfigureEndpoints(endpoints);
            }
            catch (Exception e)
            {
                Logger.Warning(e, "Configure Endpoints failed for plugin {configuration}", configuration);
            }              
        });
    }
}