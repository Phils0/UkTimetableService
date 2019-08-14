using System;
using System.Collections.Generic;
using System.Linq;
using ReflectionMagic;
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

            var originTimetable = testSchedule.Locations.First().Station.Timetable;
 
            AssertNoArrivalTime(originTimetable, TestSchedules.Ten);

            var departures = GetDepartureTimes(originTimetable);
            var service = departures.GetService(TestSchedules.Ten);
            Assert.Equal(testSchedule.Service, service);
        }

        private PublicSchedule GetDepartureTimes(LocationTimetable timetable)
        {
            var departures = (PublicSchedule) timetable.AsDynamic()._departures.RealObject;
            return departures;
        }
        
        private PublicSchedule GetArrivalTimes(LocationTimetable timetable)
        {
            var arrivals = (PublicSchedule) timetable.AsDynamic()._arrivals.RealObject;
            return arrivals;
        }
        
        private void AssertNoArrivalTime(LocationTimetable timetable, Time time)
        {
            var arrivals = GetArrivalTimes(timetable);
            Assert.Empty(arrivals.GetServices(time));
        }

        [Fact]
        public void LocationHasEntryForDestination()
        {
            var testSchedule = TestSchedules.CreateScheduleWithService();

            var destinationTimetable = testSchedule.Locations.Last().Station.Timetable;

            var arrivals = GetArrivalTimes(destinationTimetable);
            var service = arrivals.GetService(TestSchedules.TenThirty);
            Assert.Equal(testSchedule.Service, service);

            AssertNoDepartureTime(destinationTimetable, TestSchedules.TenThirty);
        }

        private void AssertNoDepartureTime(LocationTimetable destinationTimetable, Time time)
        {
            var departures = GetDepartureTimes(destinationTimetable);
            Assert.Empty(departures.GetServices(time));
        }

        private static Time TenFifteen => new Time(new TimeSpan(10, 15,0 )); 
        private static Time TenSixteen => TenFifteen.Add(TestScheduleLocations.OneMinute); 

        [Fact]
        public void LocationHasEntryForStop()
        {
            var locations = TestSchedules.DefaultLocations;
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Station.Timetable;

            var arrivals = GetArrivalTimes(stop);
            var service = arrivals.GetService(TenFifteen);
            Assert.Equal(testSchedule.Service, service);

            var departures = GetDepartureTimes(stop);
            service = departures.GetService(TenSixteen);
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
            
            AssertNoArrivalTime(stop, TenFifteen);

            var departures = GetDepartureTimes(stop);
            var service = departures.GetService(TenFifteen);
            Assert.Equal(testSchedule.Service, service);
        }
        
        [Fact]
        public void LocationHasArrivalOnlyForSetdownOnlyStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreateSetdownOnlyStop(TestStations.ClaphamJunction, TenFifteen),
                TestScheduleLocations.CreateDestination(TestStations.Waterloo, TestSchedules.TenThirty)
            };
            
            var testSchedule = TestSchedules.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Station.Timetable;

            var arrivals = GetArrivalTimes(stop);
            var service = arrivals.GetService(TenFifteen);
            Assert.Equal(testSchedule.Service, service);
            
            AssertNoDepartureTime(stop, TenFifteen);
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

            AssertNoArrivalTime(passing, TenFifteen);
            AssertNoDepartureTime(passing, TenFifteen);
        }
        
        [Fact]
        public void LocationHasOnlyOneEntryForMultipleSchedules()
        {
            var permanent = TestSchedules.CreateScheduleWithService();
            var overlay = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, service: permanent.Service);
            
            var destination = permanent.Locations.Last().Station.Timetable;

            var arrivals = GetArrivalTimes(destination);
            var services = arrivals.GetServices(TestSchedules.TenThirty);
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

            var arrivals = GetArrivalTimes(destination);
            var services = arrivals.GetServices(TestSchedules.TenThirty);
            Assert.Equal(2, services.Length);
        }
    }
}