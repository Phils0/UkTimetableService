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
        private static readonly DateTime Aug12 = Ten.Date;
        
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
        [InlineData("SUR", FindStatus.Success)]
        [InlineData("NOT_EXIST", FindStatus.LocationNotFound)]
        [InlineData("", FindStatus.LocationNotFound)]
        [InlineData(null, FindStatus.LocationNotFound)]
        public void AllDeparture(string threeLetterCode, FindStatus found)
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.AllDepartures(threeLetterCode, Aug12, GatherFilterFactory.NoFilter, false);
            
            Assert.Equal(found, find.status);
            if(found == FindStatus.Success)
                Assert.NotEmpty(find.services);
            else
                Assert.Empty(find.services);
        }

        [Fact]
        public void AllDeparturesIgnoresTime()
        {
            var expectedFirstTime = new TimeSpan(0, 7, 0);
            var expectedFirstDate = Aug12;

            var data = TestData.CreateTimetabledLocations();
            var find = data.AllDepartures("SUR", Ten, GatherFilterFactory.NoFilter, false);
            
            var first = find.services.First();
            AssertDeparture(expectedFirstDate, expectedFirstTime, first);
        }
        
        public static TheoryData<bool, DateTime, TimeSpan, DateTime, TimeSpan> FirstAndLastDepartureTimes =>
            new TheoryData<bool, DateTime, TimeSpan, DateTime, TimeSpan>()
            {
                { false, Aug12, new TimeSpan(0, 7, 0), Aug12, new TimeSpan(23, 52, 0) }, 
                { true, Aug12, new TimeSpan(2, 37, 0), Aug12.AddDays(1), new TimeSpan(2, 22, 0) }
            };
        
        [Theory]
        [MemberData(nameof(FirstAndLastDepartureTimes))]
        public void AllDepartureTimes(bool useRailDay, DateTime expectedFirstDate, TimeSpan expectedFirstTime, DateTime expectedLastDate, TimeSpan expectedLastTime)
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.AllDepartures("SUR", Aug12, GatherFilterFactory.NoFilter, useRailDay);

            var first = find.services.First();
            AssertDeparture(expectedFirstDate, expectedFirstTime, first);
            
            var last = find.services.Last();
            AssertDeparture(expectedLastDate, expectedLastTime, last);
        }
        
        private static void AssertDeparture(DateTime expectedDate, TimeSpan expectedTime, ResolvedServiceStop stop)
        {
            var origin = stop.Stop as ScheduleOrigin;
            Assert.Equal(expectedTime, origin.Departure.Value);
            Assert.Equal(expectedDate, stop.On);
        }
        
        [Fact]
        public void EmptyDeparturesWhenNone()
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.AllDepartures("WAT", Aug12, GatherFilterFactory.NoFilter, false);
            
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
        
        [Theory]
        [InlineData("WAT", FindStatus.Success)]
        [InlineData("NOT_EXIST", FindStatus.LocationNotFound)]
        [InlineData("", FindStatus.LocationNotFound)]
        [InlineData(null, FindStatus.LocationNotFound)]
        public void AllArrivals(string threeLetterCode, FindStatus found)
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.AllArrivals(threeLetterCode, Aug12, GatherFilterFactory.NoFilter, false);
            
            Assert.Equal(found, find.status);
            if(found == FindStatus.Success)
                Assert.NotEmpty(find.services);
            else
                Assert.Empty(find.services);
        }
        
        [Fact]
        public void AllArrivalsIgnoresTime()
        {
            // First arrival is from service that departed day before
            var expectedFirstTime = new TimeSpan(1,0, 2, 0);
            var expectedFirstDate = Aug12.AddDays(-1);

            var data = TestData.CreateTimetabledLocations();
            var find = data.AllArrivals("WAT", Ten, GatherFilterFactory.NoFilter, false);
            
            var first = find.services.First();
            AssertArrival(expectedFirstDate, expectedFirstTime, first);
        }
        
        public static TheoryData<bool, DateTime, TimeSpan, DateTime, TimeSpan> FirstAndLastArrivalTimes =>
            new TheoryData<bool, DateTime, TimeSpan, DateTime, TimeSpan>()
            {
                // First arrival is from service that departed day before
                { false, Aug12.AddDays(-1), new TimeSpan(1,0, 2, 0), Aug12, new TimeSpan(23, 47, 0) }, 
                { true, Aug12, new TimeSpan(2, 32, 0), Aug12.AddDays(1), new TimeSpan(2, 17, 0) }
            };
        
        [Theory]
        [MemberData(nameof(FirstAndLastArrivalTimes))]
        public void AllArrivalTimes(bool useRailDay, DateTime expectedFirstDate, TimeSpan expectedFirstTime, DateTime expectedLastDate, TimeSpan expectedLastTime)
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.AllArrivals("WAT", Aug12, GatherFilterFactory.NoFilter, useRailDay);

            var first = find.services.First();
            AssertArrival(expectedFirstDate, expectedFirstTime, first);

            var last = find.services.Last();
            AssertArrival(expectedLastDate, expectedLastTime, last);
        }

        private static void AssertArrival(DateTime expectedDate, TimeSpan expectedTime, ResolvedServiceStop stop)
        {
            var destination = stop.Stop as ScheduleDestination;
            Assert.Equal(expectedTime, destination.Arrival.Value);
            Assert.Equal(expectedDate, stop.On);
        }

        [Fact]
        public void EmptyArrivalsWhenNone()
        {
            var data = TestData.CreateTimetabledLocations();
            var find = data.AllArrivals("SUR", Aug12, GatherFilterFactory.NoFilter, false);
            
            Assert.Equal(FindStatus.NoServicesForLocation, find.status);
            Assert.Empty(find.services);
        }
    }
}