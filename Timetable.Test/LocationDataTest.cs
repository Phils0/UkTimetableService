using System;
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
        public void AddUnknownTiplocsAsNotActive()
        {
            var data = TestData.Locations;
            data.UpdateLocationNlc("NOTFOUND", "123456");

            var location = data.LocationsByTiploc["NOTFOUND"];
            Assert.False(location.IsActive);
        }

        [Theory]
        [InlineData("SURBITN", true)]
        [InlineData("NOT_EXIST", false)]
        [InlineData("WATRLOW", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void FindLocation(string tiploc, bool found)
        {
            var data = TestData.Locations;
            var find = data.TryGetLocation(tiploc, out Location location);
            
            Assert.Equal(found, find);
            if(found)
                Assert.Equal(tiploc, location.Tiploc);
        }
        
        [Theory]
        [InlineData("SUR", true)]
        [InlineData("SURBITN", false)]
        [InlineData("NOT_EXIST", false)]
        [InlineData("WAT", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void FindStation(string threeLetterCode, bool found)
        {
            var data = TestData.Locations;
            var find = data.TryGetLocation(threeLetterCode, out Station location);
            
            Assert.Equal(found, find);
            if(found)
                Assert.Equal(threeLetterCode, location.ThreeLetterCode);
        }


        private static readonly DateTime Ten = new DateTime(2019, 8, 12, 10, 0, 0);
        
        [Theory]
        [InlineData("SUR", FindStatus.Success)]
        [InlineData("NOT_EXIST", FindStatus.LocationNotFound)]
        [InlineData("", FindStatus.LocationNotFound)]
        [InlineData(null, FindStatus.LocationNotFound)]
        public void FindDeparture(string threeLetterCode, FindStatus found)
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.FindDepartures(threeLetterCode, Ten, GathererConfig.OneService);
            
            Assert.Equal(found, find.status);
            if(found == FindStatus.Success)
                Assert.NotEmpty(find.services);
            else
                Assert.Empty(find.services);
        }

        [Fact]
        public void DoNotFindDeparturesWhenNone()
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.FindDepartures("WAT", Ten, GathererConfig.OneService);
            
            Assert.Equal(FindStatus.NoServicesForLocation, find.status);
            Assert.Empty(find.services);
        }
        
        [Theory]
        [InlineData("WAT", FindStatus.Success)]
        [InlineData("NOT_EXIST", FindStatus.LocationNotFound)]
        [InlineData("", FindStatus.LocationNotFound)]
        [InlineData(null, FindStatus.LocationNotFound)]
        public void FindArrivals(string threeLetterCode, FindStatus found)
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.FindArrivals(threeLetterCode, Ten, GathererConfig.OneService);
            
            Assert.Equal(found, find.status);
            if(found == FindStatus.Success)
                Assert.NotEmpty(find.services);
            else
                Assert.Empty(find.services);
        }

        [Fact]
        public void DoNotFindArrivalsWhenNone()
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.FindArrivals("SUR", Ten, GathererConfig.OneService);
            
            Assert.Equal(FindStatus.NoServicesForLocation, find.status);
            Assert.Empty(find.services);
        }
    }
}