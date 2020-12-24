using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.Web.Loaders;
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
            var loader = CreateCifLoader();
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = await loader.LoadStationMasterListAsync(CancellationToken.None)
            };
            
            var knowledgebaseLoader = CreateKnowledgebaseLoader();
            data = await knowledgebaseLoader.EnrichReferenceDataAsync(data, CancellationToken.None);
            
            var locations = data.Locations as LocationData;
            Assert.NotEmpty(locations.Locations.Values.Where(l => !string.IsNullOrEmpty(l.Name)));
        }

        [Fact]
        public async Task StationsHaveNamesFromDarwin()
        {
            var loader = CreateCifLoader();
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = await loader.LoadStationMasterListAsync(CancellationToken.None)
            };
            
            var darwinLoader = await CreateDarwinLoader();
            data = await darwinLoader.EnrichReferenceDataAsync(data, CancellationToken.None);
            
            var locations = data.Locations as LocationData;
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
        
        private static IDataEnricher CreateKnowledgebaseLoader()
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
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = new LocationData(new List<Location>(), Substitute.For<ILogger>())
            };
            
            var loader = CreateKnowledgebaseLoader();
            data = await loader.EnrichReferenceDataAsync(data,  CancellationToken.None);
            
            Assert.NotEmpty(data.Tocs);
        }
        
        private static async Task<IDataEnricher> CreateDarwinLoader()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration(), Substitute.For<ILogger>());
            return await Factory.CreateDarwinLoader(config, Substitute.For<ILogger>());;
        }
        
        [Fact]
        public async Task LoadDarwinTocs()
        {
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = new LocationData(new List<Location>(), Substitute.For<ILogger>())
            };
            
            var loader = await CreateDarwinLoader();
            data = await loader.EnrichReferenceDataAsync(data,  CancellationToken.None);
            
            Assert.NotEmpty(data.Tocs);
        }
    }
}