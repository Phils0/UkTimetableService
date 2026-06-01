using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.Web.Loaders;
using Timetable.Web.Test.Loaders;
using Timetable.Web.Test.Mapping.Darwin;
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
        
        [Fact]
        public async Task StationsHaveViaTextFromDarwin()
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
            Assert.NotEmpty(locations.Locations.Values.Where(s => StationMapperTest.GetViaRules(s).Any()));
        }
        
        private static ICifLoader CreateCifLoader()
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration(), Substitute.For<ILogger>());
            var archive = Factory.CreateArchive(config, Substitute.For<ILogger>());
            var filters = Factory.CreateFilters(config, Substitute.For<ILogger>());
            return Factory.CreateCifLoader(archive, Substitute.For<ILogger>(), filters);
        }
        
        private static IDataLoader Create(string stationGroupsFile = null)
        {
            var config = new Configuration(
                ConfigurationHelper.GetConfiguration(stationGroupsFile: stationGroupsFile),
                Substitute.For<ILogger>());
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

            var services = data.Timetable.AsDynamic()._timetableUidMap.RealObject as Dictionary<string, IService>;
            Assert.NotEmpty(services);
            
            var retailServiceIdMap = data.Timetable.AsDynamic()._retailServiceIdMap.RealObject as Dictionary<string, IList<IService>>;
            Assert.NotEmpty(retailServiceIdMap);
        }
        
        [Fact]
        public async Task LoadKnowledgebaseTocs()
        {  
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = new LocationData(
                    new List<Location>(), 
                    Substitute.For<ILogger>(),
                    Timetable.Test.Data.Filters.Instance)
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
                Locations = new LocationData(
                    new List<Location>(),
                    Substitute.For<ILogger>(),
                    Timetable.Test.Data.Filters.Instance)
            };

            var loader = await CreateDarwinLoader();
            data = await loader.EnrichReferenceDataAsync(data,  CancellationToken.None);

            Assert.NotEmpty(data.Tocs);
        }

        [Fact]
        public async Task LoadStationGroupsIsEmptyWhenNoFile()
        {
            // The core repo doesn't ship Data/station-groups.json - the wrapper produces it at build time.
            // The full loader chain should populate data.StationGroups with an empty lookup (not null) and
            // leave the rest of the data load unaffected, proving the wiring through Factory.CreateLoader
            // → DataLoader.LoadAsync → StationGroupsLoader → Data.StationGroups end-to-end.
            var loader = Create();
            var data = await loader.LoadAsync(CancellationToken.None);

            Assert.NotNull(data.StationGroups);
            Assert.False(data.StationGroups.TryGet("GB@LO", out _));
        }

        [Fact]
        public async Task LoadStationGroupsFromFile()
        {
            // Write a station-groups file with real CRS codes that the test CIF (ttis144.zip) contains, then
            // load it through the full data loader chain. Proves the loader resolves member CRS against the
            // loaded LocationData and the result lands on Data.StationGroups - the integration of the file-
            // loaded path that the unit tests cover with mocked locations.
            //
            // Priority entries are deliberately listed in reverse priority order in JSON so that the loader's
            // sort-by-Priority is observable from the assertion below.
            var path = Path.Combine(Path.GetTempPath(), $"integration-station-groups-{Guid.NewGuid():N}.json");
            File.WriteAllText(path,
                @"{ ""groups"": [
                    { ""code"": ""GB@TEST"",
                      ""members"": [""EUS"", ""KGX"", ""LST"", ""PAD"", ""WAT""],
                      ""priorities"": [
                          { ""crs"": ""EUS"", ""priority"": 1 },
                          { ""crs"": ""WAT"", ""priority"": 0 }
                      ] }
                ] }");
            try
            {
                var loader = Create(stationGroupsFile: path);
                var data = await loader.LoadAsync(CancellationToken.None);

                Assert.True(data.StationGroups.TryGet("GB@TEST", out var group));

                // Members is an unordered set; sort before comparing.
                var memberCrs = group.Members.Select(s => s.ThreeLetterCode).OrderBy(s => s).ToArray();
                Assert.Equal(new[] { "EUS", "KGX", "LST", "PAD", "WAT" }, memberCrs);

                // Priorities is an ordered list (first-match-wins); assert the sequence to pin the contract.
                Assert.NotNull(group.Priorities);
                Assert.Equal(new[] { "WAT", "EUS" }, group.Priorities.Select(p => p.ThreeLetterCode).ToArray());
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}