using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.Web.Mapping;
using Timetable.Web.Mapping.Cif;
using Xunit;

namespace Timetable.Web.Test
{
    public class DataLoaderIntegrationTest
    {
        private static readonly MapperConfiguration _mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfile<FromCifProfile>());
        
        [Fact]
        public async Task LoadStations()
        {
            var config = new LoaderConfig(ConfigurationHelper.GetConfiguration());
            
            var factory = new Factory(_mapperConfig, config, Substitute.For<ILogger>());
            var loader = factory.CreateDataLoader();

            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None);
            
            Assert.NotEmpty(locations);
        }
        
        [Fact]
        public async Task LoadCif()
        {
            var config = new LoaderConfig(ConfigurationHelper.GetConfiguration());
            
            var factory = new Factory(_mapperConfig, config, Substitute.For<ILogger>());
            var loader = factory.CreateDataLoader();

            var data = await loader.LoadAsync(CancellationToken.None);
            var locationData = data.Locations;
            
            Assert.NotEmpty(locationData.Locations);
            Assert.NotEmpty(locationData.LocationsByTiploc);

            var services = data.Timetable.AsDynamic()._timetableUidMap.RealObject as Dictionary<string, Service>;
            Assert.NotEmpty(services);
            
            var retailServiceIdMap = data.Timetable.AsDynamic()._retailServiceIdMap.RealObject as Dictionary<string, IList<Service>>;
            Assert.NotEmpty(retailServiceIdMap);
        }
    }
}