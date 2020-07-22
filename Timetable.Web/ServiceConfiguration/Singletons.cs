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
        public ILogger Logger { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogger>(Logger);
            services.AddSingleton<IFilterFactory>(new GatherFilterFactory(Logger));
            
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