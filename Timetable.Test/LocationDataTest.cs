using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class LocationDataTest
    {
        [Fact]
        public void LoadMasterStations()
        {
            var data = TestData.Locations;
            
            Assert.Equal(3, data.Locations.Count);

            var surbiton = data.Locations["SUR"];
            Assert.Equal(1, surbiton.Locations.Count);

            var waterloo = data.Locations["WAT"];
            Assert.Equal(2, waterloo.Locations.Count);
        }
        
        [Fact]
        public void LoadMasterLocations()
        {
            var data = TestData.Locations;
            
            Assert.Equal(5, data.LocationsByTiploc.Count);

            var surbiton = data.LocationsByTiploc["SURBITN"];
            Assert.Equal("SUR", surbiton.ThreeLetterCode);

            var waterlooWindsor = data.LocationsByTiploc["WATRLOW"];
            Assert.Equal(InterchangeStatus.SubsidiaryLocation, waterlooWindsor.InterchangeStatus);
        }

        [Fact]
        public void UpdateNlc()
        {
            var data = TestData.Locations;
            data.UpdateLocationNlc("SURBITN", "123456");
            
            var surbiton = data.LocationsByTiploc["SURBITN"];
            Assert.Equal("123456", surbiton.Nlc);
            
            var surbitonStation = data.Locations["SUR"];
            Assert.Equal("1234", surbitonStation.Nlc);
        }
        
        [Fact]
        public void DoNotUpdateAnythingIfCannotFindTiploc()
        {
            var data = TestData.Locations;
            data.UpdateLocationNlc("NOTFOUND", "123456");
         
            Assert.DoesNotContain(data.LocationsByTiploc.Values, l => l.Nlc == "123456");
        }

        [Theory]
        [InlineData("SURBITN", true)]
        [InlineData("NOT_EXIST", false)]
        [InlineData("WATRLOW", true)]
        public void FindLocation(string tiploc, bool found)
        {
            var data = TestData.Locations;
            var find = data.TryGetLocation(tiploc, out var location);
            
            Assert.Equal(found, find);
            if(found)
                Assert.Equal(tiploc, location.Tiploc);
        }
    }
}