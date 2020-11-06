using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NreKnowledgebase;
using NSubstitute;
using Serilog;
using Timetable.DataLoader;
using Timetable.Web.Loaders;
using Timetable.Web.Test.Knowledgebase;
using Xunit;

namespace Timetable.Web.Test.Loaders
{
    public class KnowledgebaseLoaderTest
    {
        private IKnowledgebaseEnhancer CreateLoader(IKnowledgebaseAsync knowledgebase)
        {
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
        public async Task UpdatesTocs()
        {
            var loader = CreateLoader(knowledgebase: MockKnowledgebase);
            var tocs = await loader.UpdateTocsAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);
            
            Assert.NotEmpty(tocs);
        }
        
        //TODO This is too simplistic, need to actually handle start and end dates but will do for now
        [Fact]
        public async Task UpdatesTocsOnlyAddsFirstFound()
        {
            var loader = CreateLoader(knowledgebase: MockKnowledgebase);
            var tocs = await loader.UpdateTocsAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);

            var vt = tocs["VT"].Single();
            Assert.Equal("Avanti West Coast", vt.Name);
        }
        
        [Fact]
        public async Task UpdateTocsDoesNotAddIfExists()
        {
            var loader = CreateLoader(knowledgebase: MockKnowledgebase);
            var tocLookup = new TocLookup(Substitute.For<ILogger>());
            tocLookup.AddOrReplace("VT", new Toc("VT", "Already added"));
            var tocs = await loader.UpdateTocsAsync(tocLookup,  CancellationToken.None);
            
            var vt = tocs["VT"].Single();
            Assert.Equal("Already added", vt.Name);
        }
        
        [Fact]
        public async Task UpdateStations()
        {
            var tocs = new TocLookup(Substitute.For<ILogger>());
            var loader = CreateLoader(knowledgebase: MockKnowledgebase);

            var locations = Timetable.Test.Data.TestData.Locations;
            locations =  await loader.UpdateLocationsWithKnowledgebaseStationsAsync(locations, tocs, CancellationToken.None);

            locations.TryGetStation("WAT", out Station waterloo);
            Assert.Equal("Waterloo", waterloo.Name);
        }
        
        [Fact]
        public async Task UpdateStationsEvenWhenSet()
        {
            var tocs = new TocLookup(Substitute.For<ILogger>());
            var loader = CreateLoader(knowledgebase: MockKnowledgebase);

            var locations = Timetable.Test.Data.TestData.Locations;
            locations.TryGetStation("SUR", out var surbiton);
            surbiton.Name = "Original";
            
            locations =  await loader.UpdateLocationsWithKnowledgebaseStationsAsync(locations, tocs, CancellationToken.None);
            
            Assert.Equal("Surbiton", surbiton.Name);
        }
    }
}