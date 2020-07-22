using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Serilog;
using Timetable.Web.ServiceConfiguration;
using Xunit;

namespace Timetable.Web.Test.ServiceConfiguration
{
    public class SingletonsTest
    {
        [Fact]
        public void AddLoggerToDIContainer()
        {
            var descriptors = ConfigureServices();
            descriptors.Should().Contain(d => d.ServiceType.Equals(typeof(ILogger)));
        }

        private static List<ServiceDescriptor> ConfigureServices()
        {
            var descriptors = new List<ServiceDescriptor>();
            var services = Substitute.For<IServiceCollection>();
            services.When(s => s.Add(Arg.Any<ServiceDescriptor>()))
                .Do(args => descriptors.Add(args[0] as ServiceDescriptor));
            var logger = Substitute.For<ILogger>();

            var configure = new Singletons();
            configure.Logger = logger;
            configure.ConfigureServices(services);
            return descriptors;
        }

        [Fact]
        public void AddFilterFactoryToDIContainer()
        {
            var descriptors = ConfigureServices();

            var factoryDescriptor = descriptors.Single(d => d.ServiceType.Equals(typeof(IFilterFactory)));
            factoryDescriptor.ImplementationInstance.Should().BeOfType<GatherFilterFactory>();
        }

        [Fact]
        public void AddResponseMapperToDIContainer()
        {
            var descriptors = ConfigureServices();

            descriptors.Should().Contain(d => d.ServiceType.Equals(typeof(IMapper)));
        }
    }
}