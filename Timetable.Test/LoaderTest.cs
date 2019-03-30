using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Timetable.Test
{
    public class LoaderTest
    {
        [Fact]
        public async Task LoadStations()
        {
            var source = Substitute.For<IDataLoader>();
            source.GetStationMasterListAsync(Arg.Any<CancellationToken>())
                .Returns(new[]
                {
                    TestLocations.Surbiton,
                    TestLocations.WaterlooMain,
                    TestLocations.WaterlooWindsor
                });
            
            var loader = new Loader(source);
            var data = await loader.Load(CancellationToken.None);
            
            Assert.Equal(2, data.Locations.Count);

            var surbiton = data.Locations["SUR"];
            Assert.Equal(1, surbiton.Locations.Count);

            var waterloo = data.Locations["WAT"];
            Assert.Equal(2, waterloo.Locations.Count);
        }
    }
}