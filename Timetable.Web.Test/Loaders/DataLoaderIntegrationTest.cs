using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifParser.Archives;
using NreKnowledgebase;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.DataLoader;
using Timetable.Web.Loaders;
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
            var loader = CreateCifLoader();
            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None) as LocationData;
            
            Assert.NotEmpty(locations.Locations);
        }
        
        [Fact]
        public async Task StationsHaveNames()
        {
            var tocs = new TocLookup(Substitute.For<ILogger>());
            var loader = CreateCifLoader();
            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None) as LocationData;
            var knowledgebaseLoader = CreateKnowledgebaseLoader();
            locations = await knowledgebaseLoader.UpdateLocationsWithKnowledgebaseStationsAsync(locations, tocs, CancellationToken.None) as LocationData;
            
            Assert.NotEmpty(locations.Locations.Values.Where(l => !string.IsNullOrEmpty(l.Name)));
        }

        private static ICifLoader CreateCifLoader()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration());
            var archive = Factory.CreateArchive(config, Substitute.For<ILogger>());
            return Factory.CreateCifLoader(archive, Substitute.For<ILogger>());
        }
        
        private static IDataLoader Create()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration());
            return Factory.CreateLoader(config, Substitute.For<ILogger>());
        }
        
        private static IKnowledgebaseEnhancer CreateKnowledgebaseLoader()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration());
            var knowledgebase = Factory.CreateKnowledgebase(config, Substitute.For<ILogger>());
            return new KnowledgebaseLoader(knowledgebase, Substitute.For<ILogger>());
        }

        [Fact]
        public async Task LoadCif()
        {
            var loader = Create();
            var data = await loader.LoadAsync(CancellationToken.None);
            var locationData = data.Locations;
            
            Assert.NotEmpty(locationData.Locations);
            Assert.NotEmpty(locationData.LocationsByTiploc);
            Assert.NotEmpty(data.Tocs);

            var services = data.Timetable.AsDynamic()._timetableUidMap.RealObject as Dictionary<string, Service>;
            Assert.NotEmpty(services);
            
            var retailServiceIdMap = data.Timetable.AsDynamic()._retailServiceIdMap.RealObject as Dictionary<string, IList<Service>>;
            Assert.NotEmpty(retailServiceIdMap);
        }
        
        [Fact]
        public async Task LoadTocs()
        {
            var loader = CreateKnowledgebaseLoader();
            var tocs = await loader.UpdateTocsWithKnowledgebaseAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);
            
            Assert.NotEmpty(tocs);
        }
    }
}