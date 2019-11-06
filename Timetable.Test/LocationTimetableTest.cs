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

            var originTimetable = testSchedule.Locations.First().Station.Timetable;
 
            AssertNoArrivalTime(originTimetable, TestSchedules.Ten);

            var departures = originTimetable.GetDepartureTimes();
            var service = departures.GetService(TestSchedules.Ten);
            Assert.Equal(testSchedule.Service, service);
        }
        
        private void AssertNoArrivalTime(LocationTimetable timetable, Time time)
        {
            var arrivals = timetable.GetArrivalTimes();
            Assert.Empty(arrivals.GetServices(time));
        }

        [Fact]
        public void LocationHasEntryForDestination()
        {
            var testSchedule = TestSchedules.CreateScheduleWithService();

            var destinationTimetable = testSchedule.Locations.Last().Station.Timetable;

            var arrivals = destinationTimetable.GetArrivalTimes();
            var service = arrivals.GetService(TestSchedules.TenThirty);
            Assert.Equal(testSchedule.Service, service);

            AssertNoDepartureTime(destinationTimetable, TestSchedules.TenThirty);
        }

        private void AssertNoDepartureTime(LocationTimetable destinationTimetable, Time time)
        {
            var departures = destinationTimetable.GetDepartureTimes();
            Assert.Empty(departures.GetServices(time));
        }

        private static Time TenFifteen => new Time(new TimeSpan(10, 15,0 )); 
        private static Time TenSixteen => TenFifteen.Add(TestScheduleLocations.OneMinute); 

        [Fact]
        public void LocationHasEntryForStop()
        {
            var locations = TestSchedules.DefaultLocations;
            
            var testSchedule = TestSchedules.CreateScheduleWithService(stops: locations);

            var stop = locations[1].Station.Timetable;

            var arrivals = stop.GetArrivalTimes();
            var service = arrivals.GetService(TenFifteen);
            Assert.Equal(testSchedule.Service, service);

            var departures = stop.GetDepartureTimes();
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
            
            var testSchedule = TestSchedules.CreateScheduleWithService(stops: locations);

            var stop = locations[1].Station.Timetable;
            
            AssertNoArrivalTime(stop, TenFifteen);

            var departures = stop.GetDepartureTimes();
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
            
            var testSchedule = TestSchedules.CreateScheduleWithService(stops: locations);

            var stop = locations[1].Station.Timetable;

            var arrivals = stop.GetArrivalTimes();
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
            
            var testSchedule = TestSchedules.CreateScheduleWithService(stops: locations);

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

            var arrivals = destination.GetArrivalTimes();
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
            var service1 = TestSchedules.CreateScheduleWithService(timetableId: "A00001", stops: locations1);
            
            var locations2 = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(surbiton, TestSchedules.Ten),
                TestScheduleLocations.CreateDestination(waterloo, TestSchedules.TenThirty)
            };             
            var service2 = TestSchedules.CreateScheduleWithService(timetableId: "A00002", stops: locations2);
            
            var destination = service1.Locations.Last().Station.Timetable;

            var arrivals = destination.GetArrivalTimes();
            var services = arrivals.GetServices(TestSchedules.TenThirty);
            Assert.Equal(2, services.Length);
        }
    }
}