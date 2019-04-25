using System;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class LocationTimetableTest
    {
        [Fact]
        public void LocationHasEntryForOrigin()
        {
            var testSchedule = TestSchedules.CreateScheduleWithService();

            var origin = testSchedule.Locations.First().Location.Timetable;

            var service = origin.GetDepartures(TestSchedules.Ten)[0];
            Assert.Equal(testSchedule.Parent, service);

            Assert.Empty(origin.GetArrivals(TestSchedules.Ten));
        }
        
        [Fact]
        public void LocationHasEntryForDestination()
        {
            var testSchedule = TestSchedules.CreateScheduleWithService();

            var destination = testSchedule.Locations.Last().Location.Timetable;

            var service = destination.GetArrivals(TestSchedules.TenThirty)[0];
            Assert.Equal(testSchedule.Parent, service);

            Assert.Empty(destination.GetDepartures(TestSchedules.TenThirty));
        }
        
        private static Time TenFifteen => new Time(new TimeSpan(10, 15,0 )); 
        private static Time TenSixteen => TenFifteen.Add(TestScheduleLocations.OneMinute); 

        [Fact]
        public void LocationHasEntryForStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreateStop(TestLocations.CLPHMJN, TenFifteen),
                TestScheduleLocations.CreateDestination(TestLocations.WaterlooMain, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Location.Timetable;

            var service = stop.GetArrivals(TenFifteen)[0];
            Assert.Equal(testSchedule.Parent, service);

            service = stop.GetDepartures(TenSixteen)[0];
            Assert.Equal(testSchedule.Parent, service);
        }
        
        [Fact]
        public void LocationHasDepartureOnlyForPickupOnlyStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreatePickupOnlyStop(TestLocations.CLPHMJN, TenFifteen),
                TestScheduleLocations.CreateDestination(TestLocations.WaterlooMain, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Location.Timetable;

            Assert.Empty(stop.GetArrivals(TenFifteen));

            var service = stop.GetDepartures(TenFifteen)[0];
            Assert.Equal(testSchedule.Parent, service);
        }
        
        [Fact]
        public void LocationHasArrivalOnlyForPickupOnlyStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreateSetdownOnlyStop(TestLocations.CLPHMJN, TenFifteen),
                TestScheduleLocations.CreateDestination(TestLocations.WaterlooMain, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Location.Timetable;

            var service = stop.GetArrivals(TenFifteen)[0];
            Assert.Equal(testSchedule.Parent, service);
            
            Assert.Empty(stop.GetDepartures(TenFifteen));
        }
        
        [Fact]
        public void LocationHasNoEntryForPassing()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreatePass(TestLocations.CLPHMJN, TenFifteen),
                TestScheduleLocations.CreateDestination(TestLocations.WaterlooMain, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var passing = locations[1].Location.Timetable;

            Assert.Empty(passing.GetArrivals(TenFifteen));
            Assert.Empty(passing.GetDepartures(TenFifteen));
        }
        
        [Fact]
        public void LocationHasOnlyOneEntryForMultipleSchedules()
        {
            var permanent = TestSchedules.CreateScheduleWithService();
            var overlay = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, service: permanent.Parent);
            
            var destination = permanent.Locations.Last().Location.Timetable;

            var services = destination.GetArrivals(TestSchedules.TenThirty);
            Assert.Equal(1, services.Length);
        }
        
        [Fact]
        public void LocationHasMultipleServicesForWhenOnSameTime()
        {
            var surbiton = TestLocations.Surbiton;
            var waterloo = TestLocations.WaterlooMain;
            
            var locations1 = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreateDestination(waterloo, TestSchedules.TenThirty)
            };            
            var service1 = TestSchedules.CreateScheduleWithService(timetableId: "A00001", locations: locations1);
            
            var locations2 = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreateDestination(waterloo, TestSchedules.TenThirty)
            };             
            var service2 = TestSchedules.CreateScheduleWithService(timetableId: "A00002", locations: locations2);
            
            var destination = service1.Locations.Last().Location.Timetable;

            var services = destination.GetArrivals(TestSchedules.TenThirty);
            Assert.Equal(2, services.Length);
        }
    }
}