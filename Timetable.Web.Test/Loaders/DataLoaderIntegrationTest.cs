using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.DataLoader;
using Xunit;

namespace Timetable.Web.Test
{
    public class DataLoaderIntegrationTest
    {
        [Fact]
        public async Task LoadStations()
        {
            var loader = CreateCifLoader();
            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None) as LocationData;
            
            Assert.NotEmpty(locations.Locations);
        }
        
        [Fact]
        public async Task StationsHaveNamesFromKnowledgebase()
        {
            var tocs = new TocLookup(Substitute.For<ILogger>());
            var loader = CreateCifLoader();
            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None) as LocationData;
            var knowledgebaseLoader = CreateKnowledgebaseLoader();
            locations = await knowledgebaseLoader.UpdateLocationsWithKnowledgebaseStationsAsync(locations, tocs, CancellationToken.None) as LocationData;
            
            Assert.NotEmpty(locations.Locations.Values.Where(l => !string.IsNullOrEmpty(l.Name)));
        }

        [Fact]
        public async Task StationsHaveNamesFromDarwin()
        {
            var tocs = new TocLookup(Substitute.For<ILogger>());
            var loader = CreateCifLoader();
            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None) as LocationData;
            var darwinLoader = await CreateDarwinLoader();
            locations = await darwinLoader.UpdateLocationsAsync(locations, tocs, CancellationToken.None) as LocationData;
            
            Assert.NotEmpty(locations.Locations.Values.Where(l => !string.IsNullOrEmpty(l.Name)));
        }
        
        private static ICifLoader CreateCifLoader()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration(), Substitute.For<ILogger>());
            var archive = Factory.CreateArchive(config, Substitute.For<ILogger>());
            return Factory.CreateCifLoader(archive, Substitute.For<ILogger>());
        }
        
        private static IDataLoader Create()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration(), Substitute.For<ILogger>());
            return Factory.CreateLoader(config, Substitute.For<ILogger>()).Result;
        }
        
        private static IKnowledgebaseEnhancer CreateKnowledgebaseLoader()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration(), Substitute.For<ILogger>());
            return Factory.CreateKnowledgebase(config, Substitute.For<ILogger>());;
        }
        
        [Fact]
        public async Task LoadCif()
        {
            var loader = Create();
            var data = await loader.LoadAsync(CancellationToken.None);
            var locationData = data.Locations;
            
            Assert.True(data.IsLoaded);
            
            Assert.NotEmpty(locationData.Locations);
            Assert.NotEmpty(locationData.LocationsByTiploc);
            Assert.NotEmpty(data.Tocs);

            var services = data.Timetable.AsDynamic()._timetableUidMap.RealObject as Dictionary<string, Service>;
            Assert.NotEmpty(services);
            
            var retailServiceIdMap = data.Timetable.AsDynamic()._retailServiceIdMap.RealObject as Dictionary<string, IList<Service>>;
            Assert.NotEmpty(retailServiceIdMap);
        }
        
        [Fact]
        public async Task LoadKnowledgebaseTocs()
        {
            var loader = CreateKnowledgebaseLoader();
            var tocs = await loader.UpdateTocsAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);
            
            Assert.NotEmpty(tocs);
        }
        
        private static async Task<IDarwinLoader> CreateDarwinLoader()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration(), Substitute.For<ILogger>());
            return await Factory.CreateDarwinLoader(config, Substitute.For<ILogger>());;
        }
        
        [Fact]
        public async Task LoadDarwinTocs()
        {
            var loader = await CreateDarwinLoader();
            var tocs = await loader.UpdateTocsAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);
            
            Assert.NotEmpty(tocs);
        }
    }
}