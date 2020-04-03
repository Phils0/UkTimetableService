using System;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class LocationFindDeparturesTest
    {
        private static readonly DateTime Aug1 = new DateTime(2019, 8, 1);
        private static readonly DateTime Aug1AtTen = new DateTime(2019, 8, 1, 10, 0 ,0);
        
        [Fact]
        public void FindDepartures()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindDepartures(Aug1AtTen, GathererConfig.Create(1, 5));
            
            Assert.Equal(6, schedules.Length);

            var first = schedules.First();
            Assert.Equal("X00585", first.Service.TimetableUid);
            Assert.Equal(Aug1, first.On);
            var last = schedules.Last();
            Assert.Equal("X00660", last.Service.TimetableUid);        
            Assert.Equal(Aug1, last.On);
        }

        [Fact]
        public void ReturnAllFoundDeparturesWhenRequestTooManyAfter()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];

            var aug31AtTen = new DateTime(2019, 8, 31, 10, 0, 0);
            var schedules = clapham.Timetable.FindDepartures(aug31AtTen, GathererConfig.Create(1, 100));
            
            Assert.Equal(57, schedules.Length);     
        }
        
        [Fact]
        public void ReturnAllFoundDeparturesWhenRequestTooManyBefore()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindDepartures(Aug1AtTen, GathererConfig.Create(100, 1));
            
            Assert.Equal(41, schedules.Length);     
        }
        
        [Fact()]
        public void FindDeparturesPreviousDay()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindDepartures(new DateTime(2019, 8, 2, 0, 30 ,0), GathererConfig.Create(5, 1));
            
            Assert.Equal(6, schedules.Length);

            var first = schedules.First().Service;
            Assert.Equal(Aug1, first.On);  
        }

        
        [Fact()]
        public void FindDeparturesNextDay()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindDepartures(new DateTime(2019, 8, 1, 23, 30 ,0), GathererConfig.Create(1, 5));
            
            Assert.Equal(6, schedules.Length);

            var last = schedules.Last().Service;
            Assert.Equal(Aug1.AddDays(1), last.On);  
        }
        
        [Fact]
        public void AllDeparturessReturnsEverything()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.AllDepartures(Aug1AtTen, GatherFilterFactory.NoFilter, Time.Midnight);

            Assert.Equal(96, schedules.Length);
        }
    }
}