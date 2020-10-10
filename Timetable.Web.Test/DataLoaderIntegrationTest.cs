using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifParser.Archives;
using NreKnowledgebase;
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
            var loader = Create();
            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None);
            
            Assert.NotEmpty(locations);
        }

        private static IDataLoader Create()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration());
            return Factory.CreateLoader(config, Substitute.For<ILogger>());
        }

        [Fact]
        public async Task LoadCif()
        {
            var loader = Create();
            var data = await loader.LoadAsync(CancellationToken.None);
            var locationData = data.Locations;
            
            Assert.NotEmpty(locationData.Locations);
            Assert.NotEmpty(locationData.LocationsByTiploc);

            var services = data.Timetable.AsDynamic()._timetableUidMap.RealObject as Dictionary<string, Service>;
            Assert.NotEmpty(services);
            
            var retailServiceIdMap = data.Timetable.AsDynamic()._retailServiceIdMap.RealObject as Dictionary<string, IList<Service>>;
            Assert.NotEmpty(retailServiceIdMap);
        }
        
        [Fact]
        public async Task LoadTocs()
        {
            var loader = Create();
            var tocs = await loader.LoadKnowledgebaseTocsAsync(CancellationToken.None);
            
            Assert.NotEmpty(tocs);
        }
    }
}