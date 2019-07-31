using System;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class LocationFindDeparturesTest
    {
        private static readonly DateTime Aug1AtTen = new DateTime(2019, 8, 1, 10, 0 ,0);
        
        [Fact]
        public void FindDepartures()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindDepartures(Aug1AtTen);
            
            Assert.Equal(6, schedules.Length);

            var first = schedules.First();
            Assert.Equal("X00585", first.TimetableUid);
            var last = schedules.Last();
            Assert.Equal("X00660", last.TimetableUid);        
        }
        
        [Fact]
        public void ReturnAllFoundDeparturesWhenRequestTooManyBefore()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindDepartures(Aug1AtTen, 100, 1);
            
            Assert.Equal(96, schedules.Length);      
        }
        
        [Fact]
        public void ReturnAllFoundDeparturesWhenRequestTooManyAfter()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindDepartures(Aug1AtTen, 1, 100);
            
            Assert.Equal(96, schedules.Length);      
        }
        
        [Fact]
        public void FindDeparturesNextDay()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.Locations["CLJ"];
            
            var schedules = clapham.Timetable.FindDepartures(new DateTime(2019, 8, 1, 23, 30 ,0));
            
            Assert.Equal(6, schedules.Length);

            var last = schedules.Last().Locations[1];
            //TODO check next day       
        }
    }
}