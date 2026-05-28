using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Serilog;
using Timetable.Web.ServiceConfiguration;
using Xunit;

namespace Timetable.Web.Test.ServiceConfiguration
{
    public class SingletonsTest
    {
        private static List<ServiceDescriptor> ConfigureServices(string optimisationStrategy = "Longest")
        {
            var descriptors = new List<ServiceDescriptor>();
            var services = Substitute.For<IServiceCollection>();
            services.When(s => s.Add(Arg.Any<ServiceDescriptor>()))
                .Do(args => descriptors.Add(args[0] as ServiceDescriptor));
            var logger = Substitute.For<ILogger>();

            var appSettings = Substitute.For<IConfiguration>();
            appSettings["StationGroupOptimisationStrategy"].Returns(optimisationStrategy);
            var configuration = new Configuration(appSettings, logger);

            var configure = new Singletons(configuration);
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

        [Theory]
        [InlineData("Longest")]
        [InlineData("Shortest")]
        public void AddStationGroupStopOptimiserToDIContainer(string optimisationStrategy)
        {
            var descriptors = ConfigureServices(optimisationStrategy);

            var optimiserDescriptor = descriptors.Single(d => d.ServiceType.Equals(typeof(IStationGroupStopOptimiser)));
            optimiserDescriptor.ImplementationInstance.Should().BeOfType<StationGroupStopOptimiser>();
        }
    }
}