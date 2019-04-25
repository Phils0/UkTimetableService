using System;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class LocationFindArrivalsTest
    {
        private static readonly DateTime Aug1AtTen = new DateTime(2019, 8, 1, 10, 0 ,0);
        
        [Fact]
        public void FindArrivals()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.LocationsByTiploc["CLPHMJN"];
            
            var schedules = clapham.Timetable.FindArrivals(Aug1AtTen);
            
            Assert.Equal(6, schedules.Length);

            var first = schedules.First();
            Assert.Equal("X00600", first.TimetableUid);
            var last = schedules.Last();
            Assert.Equal("X00525", last.TimetableUid);        
        }
        
        [Fact]
        public void ReturnAllFoundArrivalsWhenRequestTooManyBefore()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.LocationsByTiploc["CLPHMJN"];
            
            var schedules = clapham.Timetable.FindArrivals(Aug1AtTen, 100, 1);
            
            Assert.Equal(96, schedules.Length);      
        }
        
        [Fact]
        public void ReturnAllFoundArrivalsWhenRequestTooManyAfter()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.LocationsByTiploc["CLPHMJN"];
            
            var schedules = clapham.Timetable.FindArrivals(Aug1AtTen, 1, 100);
            
            Assert.Equal(96, schedules.Length);      
        }
        
        [Fact]
        public void FindArrivalsNextDay()
        {
            var locations = TestData.CreateTimetabledLocations();
            var clapham = locations.LocationsByTiploc["CLPHMJN"];
            
            var schedules = clapham.Timetable.FindArrivals(new DateTime(2019, 8, 2, 0, 30 ,0));
            
            Assert.Equal(6, schedules.Length);

            var last = schedules.Last().Locations[1];
            //TODO check next day       
        }
    }
}