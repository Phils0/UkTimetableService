using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CifParser.Archives;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Serilog;
using Timetable.DataLoader;
using Timetable.Web.ServiceConfiguration;
using Xunit;

namespace Timetable.Web.Test.ServiceConfiguration
{
    public class SetDataTest
    {
        [Fact]
        public void AddLocations()
        {
            var descriptors = ConfigureServices();

            var dataDescription = descriptors.Single(d => d.ServiceType.Equals(typeof(ILocationData)));
            dataDescription.ImplementationInstance.Should().NotBeNull();
        }
        
        private List<ServiceDescriptor> ConfigureServices()
        {
            var descriptors = new List<ServiceDescriptor>();
            
            var logger = Substitute.For<ILogger>();
            var loader = CreateDataLoader(logger);

            var services = Substitute.For<IServiceCollection>();
            services.When(s => s.Add(Arg.Any<ServiceDescriptor>()))
                .Do(args => descriptors.Add(args[0] as ServiceDescriptor));

            var setData = new SetData(loader, logger);
            setData.ConfigureServices(services);
            return descriptors;
        }

        private IDataLoader CreateDataLoader(ILogger logger, Data data = null)
        {
            data = data ?? CreateDummyData(logger);
            var loader = Substitute.For<IDataLoader>();
            loader.LoadAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(data));
            return loader;
        }

        private Data CreateDummyData(ILogger logger)
        {
            var data = new Data();
            data.Locations = new LocationData(new List<Location>(), logger);
            data.Timetable = new TimetableData(logger);
            data.Tocs = new TocLookup(logger);
            return data;
        }
        
        [Fact]
        public void AddTimetable()
        {
            var descriptors = ConfigureServices();

            var dataDescription = descriptors.Single(d => d.ServiceType.Equals(typeof(ITimetable)));
            dataDescription.ImplementationInstance.Should().NotBeNull();
        }
        
        [Fact]
        public void AddServiceConfiguration()
        {
            var descriptors = ConfigureServices();

            var dataDescription = descriptors.Single(d => d.ServiceType.Equals(typeof(Model.Configuration)));
            dataDescription.ImplementationInstance.Should().NotBeNull();
        }
        
        [Fact]
        public void HasLoadedData()
        {
            var logger = Substitute.For<ILogger>();
            var loader = CreateDataLoader(logger);

            var services = Substitute.For<IServiceCollection>();

            var setData = new SetData(loader, logger);

            setData.HasLoadedData.Should().BeFalse();
            setData.ConfigureServices(services);
            setData.HasLoadedData.Should().BeTrue();
        }
    }
}