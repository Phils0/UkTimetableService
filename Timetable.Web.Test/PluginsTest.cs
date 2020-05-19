using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Serilog;
using Timetable.Web.Plugin;
using Xunit;

namespace Timetable.Web.Test
{
    public class PluginsTest
    {
        [Fact]
        public void FindSwaggerPlugin()
        {
            var plugins = Plugins.FindPlugins(AppContext.BaseDirectory, Substitute.For<ILogger>());
            Assert.Single(plugins.Values);
            Assert.IsType<SwaggerPlugin>(plugins.Values[0]);
        }

        [Fact]
        public void ConfigureServicesOnAllPlugins()
        {
            var plugin1 = Substitute.For<IPlugin>();
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new Plugins(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.ConfigureServices(Substitute.For<IServiceCollection>());

            plugin1.ReceivedWithAnyArgs().ConfigureServices(Arg.Any<IServiceCollection>(), Arg.Any<ILogger>());
            plugin2.ReceivedWithAnyArgs().ConfigureServices(Arg.Any<IServiceCollection>(), Arg.Any<ILogger>());
        }

        [Fact]
        public void ConfigureServicesOnAllPluginsWhenOneThrowsException()
        {
            var plugin1 = Substitute.For<IPlugin>();
            plugin1.When(p => p.ConfigureServices(Arg.Any<IServiceCollection>(), Arg.Any<ILogger>()))
                .Do(x => { throw new Exception("Fail Plugin1"); });
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new Plugins(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.ConfigureServices(Substitute.For<IServiceCollection>());

            plugin1.ReceivedWithAnyArgs().ConfigureServices(Arg.Any<IServiceCollection>(), Arg.Any<ILogger>());
            plugin2.ReceivedWithAnyArgs().ConfigureServices(Arg.Any<IServiceCollection>(), Arg.Any<ILogger>());
        }

        [Fact]
        public void ConfigureAllPlugins()
        {
            var plugin1 = Substitute.For<IPlugin>();
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new Plugins(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.Configure(Substitute.For<IApplicationBuilder>(), Substitute.For<IWebHostEnvironment>());

            plugin1.ReceivedWithAnyArgs().Configure(Substitute.For<IApplicationBuilder>(),
                Substitute.For<IWebHostEnvironment>(), Arg.Any<ILogger>());
            plugin2.ReceivedWithAnyArgs().Configure(Substitute.For<IApplicationBuilder>(),
                Substitute.For<IWebHostEnvironment>(), Arg.Any<ILogger>());
        }

        [Fact]
        public void ConfigureAllPluginsWhenOneThrowsException()
        {
            var plugin1 = Substitute.For<IPlugin>();
            plugin1.When(p =>
                    p.Configure(Arg.Any<IApplicationBuilder>(), Arg.Any<IWebHostEnvironment>(), Arg.Any<ILogger>()))
                .Do(x => { throw new Exception("Fail Plugin1"); });
            var plugin2 = Substitute.For<IPlugin>();

            var plugins = new Plugins(new[] {plugin1, plugin2}, Substitute.For<ILogger>());

            plugins.Configure(Substitute.For<IApplicationBuilder>(), Substitute.For<IWebHostEnvironment>());

            plugin1.ReceivedWithAnyArgs().Configure(Substitute.For<IApplicationBuilder>(),
                Substitute.For<IWebHostEnvironment>(), Arg.Any<ILogger>());
            plugin2.ReceivedWithAnyArgs().Configure(Substitute.For<IApplicationBuilder>(),
                Substitute.For<IWebHostEnvironment>(), Arg.Any<ILogger>());
        }
    }
}