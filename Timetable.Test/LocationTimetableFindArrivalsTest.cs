using System;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class LocationFindArrivalsTest
    {
        private static readonly DateTime Aug1 = new DateTime(2019, 8, 1);
        private static readonly DateTime Aug1AtTen = new DateTime(2019, 8, 1, 10, 0 ,0);
        
        [Fact]
        public void FindArrivals()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindArrivals(Aug1AtTen, GathererConfig.Create(3, 3));
            
            Assert.Equal(6, schedules.Length);

            var first = schedules.First();
            Assert.Equal("X00555", first.Details.TimetableUid);
            Assert.Equal(Aug1, first.On);
            var last = schedules.Last();
            Assert.Equal("X00630", last.Details.TimetableUid);        
            Assert.Equal(Aug1, last.On);
        }
        
        [Fact]
        public void ReturnAllFoundArrivalsWhenRequestTooManyBefore()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindArrivals(Aug1AtTen, GathererConfig.Create(100, 1));
            
            Assert.Equal(41, schedules.Length);    //TODO Handle wrapping day 
        }

        [Fact]
        public void ReturnAllFoundArrivalsWhenRequestTooManyAfter()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindArrivals(Aug1AtTen, GathererConfig.Create(1, 100));

            Assert.Equal(57, schedules.Length);     //TODO Handle wrapping day    
        }
        
        [Fact(Skip="Not implemented yet")]
        public void FindArrivalsNextDay()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindArrivals(new DateTime(2019, 8, 2, 0, 30 ,0), GathererConfig.Create(3, 3));
            
            Assert.Equal(6, schedules.Length);

            var last = schedules.Last().Details.Locations[1];
            //TODO check next day       
        }
        
        [Fact]
        public void AllArrivalsReturnsEverything()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.AllArrivals(Aug1AtTen, GatherFilterFactory.NoFilter, Time.Midnight);

            Assert.Equal(96, schedules.Length);
        }
    }
}