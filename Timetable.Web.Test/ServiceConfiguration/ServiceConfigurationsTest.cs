using System;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Serilog;
using Timetable.Web.Plugin;
using Timetable.Web.ServiceConfiguration;
using Xunit;

namespace Timetable.Web.Test.ServiceConfiguration
{
    public class ServiceConfigurationsTest
    {
        [Fact]
        public void ConfigureServicesOnAllPlugins()
        {
            var plugin1 = Substitute.For<IPlugin>();
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new ServiceConfigurations(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.ConfigureServices(Substitute.For<IServiceCollection>());

            plugin1.ReceivedWithAnyArgs().ConfigureServices(Arg.Any<IServiceCollection>());
            plugin2.ReceivedWithAnyArgs().ConfigureServices(Arg.Any<IServiceCollection>());
        }

        [Fact]
        public void ConfigureServicesOnAllPluginsWhenOneThrowsException()
        {
            var plugin1 = Substitute.For<IPlugin>();
            plugin1.When(p => p.ConfigureServices(Arg.Any<IServiceCollection>()))
                .Do(x => { throw new Exception("Fail Plugin1"); });
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new ServiceConfigurations(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.ConfigureServices(Substitute.For<IServiceCollection>());

            plugin1.ReceivedWithAnyArgs().ConfigureServices(Arg.Any<IServiceCollection>());
            plugin2.ReceivedWithAnyArgs().ConfigureServices(Arg.Any<IServiceCollection>());
        }

        [Fact]
        public void ConfigureAllPlugins()
        {
            var plugin1 = Substitute.For<IPlugin>();
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new ServiceConfigurations(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.Configure(Substitute.For<IApplicationBuilder>(), Substitute.For<IWebHostEnvironment>());

            plugin1.ReceivedWithAnyArgs().Configure(Substitute.For<IApplicationBuilder>(),
                Substitute.For<IWebHostEnvironment>());
            plugin2.ReceivedWithAnyArgs().Configure(Substitute.For<IApplicationBuilder>(),
                Substitute.For<IWebHostEnvironment>());
        }

        [Fact]
        public void ConfigureAllPluginsWhenOneThrowsException()
        {
            var plugin1 = Substitute.For<IPlugin>();
            plugin1.When(p =>
                    p.Configure(Arg.Any<IApplicationBuilder>(), Arg.Any<IWebHostEnvironment>()))
                .Do(x => { throw new Exception("Fail Plugin1"); });
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new ServiceConfigurations(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.Configure(Substitute.For<IApplicationBuilder>(), Substitute.For<IWebHostEnvironment>());

            plugin1.ReceivedWithAnyArgs().Configure(Substitute.For<IApplicationBuilder>(),
                Substitute.For<IWebHostEnvironment>());
            plugin2.ReceivedWithAnyArgs().Configure(Substitute.For<IApplicationBuilder>(),
                Substitute.For<IWebHostEnvironment>());
        }
        
        [Fact]
        public void ConfigureEndpointsOnAllPlugins()
        {
            var plugin1 = Substitute.For<IPlugin>();
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new ServiceConfigurations(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.ConfigureEndpoints(Substitute.For<IEndpointRouteBuilder>());

            plugin1.ReceivedWithAnyArgs().ConfigureEndpoints(Arg.Any<IEndpointRouteBuilder>());
            plugin2.ReceivedWithAnyArgs().ConfigureEndpoints(Arg.Any<IEndpointRouteBuilder>());
        }

        [Fact]
        public void ConfigureEndpointsOnAllPluginsWhenOneThrowsException()
        {
            var plugin1 = Substitute.For<IPlugin>();
            plugin1.When(p => p.ConfigureEndpoints(Arg.Any<IEndpointRouteBuilder>()))
                .Do(x => { throw new Exception("Fail Plugin1"); });
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new ServiceConfigurations(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.ConfigureEndpoints(Substitute.For<IEndpointRouteBuilder>());

            plugin1.ReceivedWithAnyArgs().ConfigureEndpoints(Arg.Any<IEndpointRouteBuilder>());
            plugin2.ReceivedWithAnyArgs().ConfigureEndpoints(Arg.Any<IEndpointRouteBuilder>());
        }
        
        [Fact]
        public void AddSetsLogger()
        {
            var plugin = Substitute.For<IPlugin>();
            var logger = Substitute.For<ILogger>();
            var configurations = new ServiceConfigurations(logger);

            configurations.Add(plugin);

            plugin.Logger.Should().Be(logger);
        }
        
        [Fact]
        public void AddRangeSetsLogger()
        {
            var plugin1 = Substitute.For<IPlugin>();            
            var plugin2 = Substitute.For<IPlugin>();
            var logger = Substitute.For<ILogger>();
            var configurations = new ServiceConfigurations(logger);

            configurations.AddRange(new [] {plugin1, plugin2});
            
            plugin1.Logger.Should().Be(logger);                
            plugin2.Logger.Should().Be(logger);  
        }
    }
}