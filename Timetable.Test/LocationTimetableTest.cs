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

            var origin = testSchedule.Locations.First().Station.Timetable;

            var service = origin.GetDepartures(TestSchedules.Ten)[0];
            Assert.Equal(testSchedule.Service, service);

            Assert.Empty(origin.GetArrivals(TestSchedules.Ten));
        }
        
        [Fact]
        public void LocationHasEntryForDestination()
        {
            var testSchedule = TestSchedules.CreateScheduleWithService();

            var destination = testSchedule.Locations.Last().Station.Timetable;

            var service = destination.GetArrivals(TestSchedules.TenThirty)[0];
            Assert.Equal(testSchedule.Service, service);

            Assert.Empty(destination.GetDepartures(TestSchedules.TenThirty));
        }
        
        private static Time TenFifteen => new Time(new TimeSpan(10, 15,0 )); 
        private static Time TenSixteen => TenFifteen.Add(TestScheduleLocations.OneMinute); 

        [Fact]
        public void LocationHasEntryForStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreateStop(TestStations.ClaphamJunction, TenFifteen),
                TestScheduleLocations.CreateDestination(TestStations.Waterloo, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Station.Timetable;

            var service = stop.GetArrivals(TenFifteen)[0];
            Assert.Equal(testSchedule.Service, service);

            service = stop.GetDepartures(TenSixteen)[0];
            Assert.Equal(testSchedule.Service, service);
        }
        
        [Fact]
        public void LocationHasDepartureOnlyForPickupOnlyStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreatePickupOnlyStop(TestStations.ClaphamJunction, TenFifteen),
                TestScheduleLocations.CreateDestination(TestStations.Waterloo, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Station.Timetable;

            Assert.Empty(stop.GetArrivals(TenFifteen));

            var service = stop.GetDepartures(TenFifteen)[0];
            Assert.Equal(testSchedule.Service, service);
        }
        
        [Fact]
        public void LocationHasArrivalOnlyForPickupOnlyStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreateSetdownOnlyStop(TestStations.ClaphamJunction, TenFifteen),
                TestScheduleLocations.CreateDestination(TestStations.Waterloo, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Station.Timetable;

            var service = stop.GetArrivals(TenFifteen)[0];
            Assert.Equal(testSchedule.Service, service);
            
            Assert.Empty(stop.GetDepartures(TenFifteen));
        }
        
        [Fact]
        public void LocationHasNoEntryForPassing()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreatePass(TestStations.ClaphamJunction, TenFifteen),
                TestScheduleLocations.CreateDestination(TestStations.Waterloo, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var passing = locations[1].Station.Timetable;

            Assert.Empty(passing.GetArrivals(TenFifteen));
            Assert.Empty(passing.GetDepartures(TenFifteen));
        }
        
        [Fact]
        public void LocationHasOnlyOneEntryForMultipleSchedules()
        {
            var permanent = TestSchedules.CreateScheduleWithService();
            var overlay = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, service: permanent.Service);
            
            var destination = permanent.Locations.Last().Station.Timetable;

            var services = destination.GetArrivals(TestSchedules.TenThirty);
            Assert.Single(services);
        }
        
        [Fact]
        public void LocationHasMultipleServicesForWhenOnSameTime()
        {
            var surbiton = TestStations.Surbiton;
            var waterloo = TestStations.Waterloo;
            
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
            
            var destination = service1.Locations.Last().Station.Timetable;

            var services = destination.GetArrivals(TestSchedules.TenThirty);
            Assert.Equal(2, services.Length);
        }
    }
}