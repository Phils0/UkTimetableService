using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Timetable.Web.Mapping;
using Timetable.Web.Plugin;

namespace Timetable.Web.ServiceConfiguration
{
    internal class Singletons : IPlugin
    {
        private readonly Configuration _config;

        public Singletons(Configuration config)
        {
            _config = config;
        }

        public ILogger Logger { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFilterFactory>(new GatherFilterFactory(Logger));

            var heuristic = _config.StationGroupOptimisationStrategy;
            var selector = new CanonicalStopSelector(heuristic);
            services.AddSingleton<IStationGroupStopOptimiser>(new StationGroupStopOptimiser(selector, Logger));
            Logger.Information("Registered station group stop optimiser using the {Heuristic} journey heuristic", heuristic);

            var mapperConfiguration = new MapperConfiguration(
                cfg => {
                    cfg.AddProfile<ToViewModelProfile>();
                });
            services.AddSingleton<IMapper>(mapperConfiguration.CreateMapper());
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