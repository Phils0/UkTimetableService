using System;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Timetable.Web.Plugin;

namespace Timetable.Web
{
    public class Plugins
    {
        public IPlugin[] Values { get; private set; }
        
        private readonly ILogger _logger;
        
        public Plugins(IPlugin[] plugins, ILogger logger)
        {
            Values = plugins ?? new IPlugin[0];
            _logger = logger;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var plugin in Values)
            {
                try
                {
                    plugin.ConfigureServices(services, _logger);
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "ConfigureServices failed for plugin {plugin}", plugin);
                }
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            foreach (var plugin in Values)
            {
                try
                {
                    plugin.Configure(app, env, _logger);
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Configure failed for plugin {plugin}", plugin);
                }
            }
        }
        
        public static Plugins FindPlugins(string path, ILogger logger)
        {
            logger.Information("Loading plugins from {path}", path);
            
            var conventions = new ConventionBuilder();
            conventions.
                ForTypesDerivedFrom<IPlugin>().
                Export<IPlugin>().
                Shared();
            
            var configuration = new ContainerConfiguration().WithAssembliesInPath(path, conventions);
            
            IPlugin[] plugins = new IPlugin[0];
            using var container = configuration.CreateContainer();
            try
            {
                plugins = container.GetExports<IPlugin>().ToArray();
            }
            catch (Exception e)
            {
                logger.Warning(e, "Did not load plugins");
            }
            return new Plugins(plugins, logger);
        }
    }
}