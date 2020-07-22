using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using AutoMapper;
using CifParser.Archives;
using Microsoft.Extensions.Configuration;
using Serilog;
using Timetable.Web.Mapping.Cif;
using Timetable.Web.Plugin;
using Timetable.Web.ServiceConfiguration;

namespace Timetable.Web
{
    internal static class ConfigurationFinder
    {
        internal static readonly string PluginDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        
        internal static ServiceConfigurations Find(Configuration config, ILogger logger = null)
        {
            logger = logger ?? Log.Logger;
            logger.Information("Loading configuration", PluginDir);
            
            var configurations = new ServiceConfigurations( logger);
            AddInternalConfigurations();
            AddExternalPlugins();
            return configurations;

            void AddInternalConfigurations()
            {
                logger.Information("Loading internal service configuration", PluginDir);
                
                var loader = Factory.CreateLoader(config, logger);
                var addData = new SetData(loader, logger);
                
                configurations.Add(new Singletons());
                configurations.Add(addData);
                if(config.EnablePrometheusMonitoring)
                    configurations.Add(new ServiceConfiguration.Prometheus());
                configurations.Add(new HealthCheck(addData, config.EnablePrometheusMonitoring));
                configurations.Add(new Swagger());
                configurations.Add(new ExceptionHandler());
            }

            void AddExternalPlugins()
            {
                if (!config.EnableCustomPlugins)
                    return;
                
                try
                {
                    logger.Information("Loading external plugins from {path}", PluginDir);
                    var conventions = new ConventionBuilder();
                    conventions.ForTypesDerivedFrom<IPlugin>().Export<IPlugin>().Shared();

                    var containerConfig = new ContainerConfiguration().WithAssembliesInPath(PluginDir, conventions);
                    
                    using var container = containerConfig.CreateContainer();  
                    
                    var externalPlugins = container.GetExports<IPlugin>();
                    configurations.AddRange(externalPlugins);
                }
                catch (Exception e)
                {
                    logger.Warning(e, "Error loading plugins from {path}", PluginDir);
                }
            }
        }
    }
}