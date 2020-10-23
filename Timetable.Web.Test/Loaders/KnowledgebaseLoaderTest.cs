using System.Threading;
using System.Threading.Tasks;
using NreKnowledgebase;
using NSubstitute;
using Serilog;
using Timetable.DataLoader;
using Timetable.Test.Data;
using Timetable.Web.Loaders;
using Timetable.Web.Test.Knowledgebase;
using Xunit;

namespace Timetable.Web.Test.Loaders
{
    public class KnowledgebaseLoaderTest
    {
        private IKnowledgebaseEnhancer CreateLoader(IKnowledgebaseAsync knowledgebase)
        {

            knowledgebase = knowledgebase ?? Substitute.For<IKnowledgebaseAsync>();
            return new KnowledgebaseLoader(knowledgebase, Substitute.For<ILogger>());
        }
        
        private IKnowledgebaseAsync MockKnowledgebase
        {
            get
            {
                var knowledgebase = Substitute.For<IKnowledgebaseAsync>();
                knowledgebase.GetTocs(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(TestTocs.Tocs));
                knowledgebase.GetStations(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Timetable.Web.Test.Knowledgebase.TestStations.Stations));
                return knowledgebase;
            }            
        }
        
        [Fact]
        public async Task LoadTocs()
        {
            var loader = CreateLoader(knowledgebase: MockKnowledgebase);
            var tocs = await loader.UpdateTocsWithKnowledgebaseAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);
            
            Assert.NotEmpty(tocs);
        }
        
        [Fact]
        public async Task UpdateStationNames()
        {
            var tocs = new TocLookup(Substitute.For<ILogger>());
            var loader = CreateLoader(knowledgebase: MockKnowledgebase);

            var locations = TestData.Locations;
            locations =  await loader.UpdateLocationsWithKnowledgebaseStationsAsync(locations, tocs, CancellationToken.None);

            locations.TryGetStation("WAT", out Station waterloo);
            Assert.Equal("Waterloo", waterloo.Name);
        }
    }
}