using System;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class LocationTest
    {
        [Fact]
        public void ToStringForNotSetReturnsNotSet()
        {
            Assert.Equal("Not Set", Location.NotSet.ToString());
        }

        [Fact]
        public void ToStringReturnsCrsAndTiploc()
        {
            Assert.Equal("WAT-WATRLMN", TestLocations.WaterlooMain.ToString());
            Assert.Equal("WAT-WATRLOW", TestLocations.WaterlooWindsor.ToString());
        }

        public static TheoryData<Location, int> LocationComparison =>
            new TheoryData<Location, int>()
            {
                {TestLocations.Surbiton, 4},
                {TestLocations.WaterlooMain, 0},
                {TestLocations.WaterlooWindsor, -2},
            };

        [Theory]
        [MemberData(nameof(LocationComparison))]
        public void Compare(Location other, int expected)
        {
            var test = TestLocations.WaterlooMain;
            Assert.Equal(expected, test.CompareTo(other));
            Assert.Equal(expected * -1, other.CompareTo(test));
        }

        [Fact]
        public void LocationHasEntryForOrigin()
        {
            var testSchedule = TestData.CreateScheduleWithService();

            var origin = testSchedule.Locations.First().Location;

            var service = origin.FindExactDepartures(TestData.Ten)[0];
            Assert.Equal(testSchedule.Parent, service);

            Assert.Empty(origin.FindExactArrivals(TestData.Ten));
        }
        
        [Fact]
        public void LocationHasEntryForDestination()
        {
            var testSchedule = TestData.CreateScheduleWithService();

            var destination = testSchedule.Locations.Last().Location;

            var service = destination.FindExactArrivals(TestData.TenThirty)[0];
            Assert.Equal(testSchedule.Parent, service);

            Assert.Empty(destination.FindExactDepartures(TestData.TenThirty));
        }
        
        private static Time TenFifteen => new Time(new TimeSpan(10, 15,0 )); 
        private static Time TenSixteen => TenFifteen.Add(TestScheduleLocations.OneMinute); 

        [Fact]
        public void LocationHasEntryForStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, TestData.Ten),
                TestScheduleLocations.CreateStop(TestLocations.CLPHMJN, TenFifteen),
                TestScheduleLocations.CreateDestination(TestLocations.WaterlooMain, TestData.TenThirty)
            };
            
            var testSchedule = TestData.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Location;

            var service = stop.FindExactArrivals(TenFifteen)[0];
            Assert.Equal(testSchedule.Parent, service);

            service = stop.FindExactDepartures(TenSixteen)[0];
            Assert.Equal(testSchedule.Parent, service);
        }
        
        [Fact]
        public void LocationHasDepartureOnlyForPickupOnlyStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, TestData.Ten),
                TestScheduleLocations.CreatePickupOnlyStop(TestLocations.CLPHMJN, TenFifteen),
                TestScheduleLocations.CreateDestination(TestLocations.WaterlooMain, TestData.TenThirty)
            };
            
            var testSchedule = TestData.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Location;

            Assert.Empty(stop.FindExactArrivals(TenFifteen));

            var service = stop.FindExactDepartures(TenFifteen)[0];
            Assert.Equal(testSchedule.Parent, service);
        }
        
        [Fact]
        public void LocationHasArrivalOnlyForPickupOnlyStop()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, TestData.Ten),
                TestScheduleLocations.CreateSetdownOnlyStop(TestLocations.CLPHMJN, TenFifteen),
                TestScheduleLocations.CreateDestination(TestLocations.WaterlooMain, TestData.TenThirty)
            };
            
            var testSchedule = TestData.CreateScheduleWithService(locations: locations);

            var stop = locations[1].Location;

            var service = stop.FindExactArrivals(TenFifteen)[0];
            Assert.Equal(testSchedule.Parent, service);
            
            Assert.Empty(stop.FindExactDepartures(TenFifteen));
        }
        
        [Fact]
        public void LocationHasNoEntryForPassing()
        {
            var locations = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, TestData.Ten),
                TestScheduleLocations.CreatePass(TestLocations.CLPHMJN, TenFifteen),
                TestScheduleLocations.CreateDestination(TestLocations.WaterlooMain, TestData.TenThirty)
            };
            
            var testSchedule = TestData.CreateScheduleWithService(locations: locations);

            var passing = locations[1].Location;

            Assert.Empty(passing.FindExactArrivals(TenFifteen));
            Assert.Empty(passing.FindExactDepartures(TenFifteen));
        }
        
        [Fact]
        public void LocationHasOnlyOneEntryForMultipleSchedules()
        {
            var permanent = TestData.CreateScheduleWithService();
            var overlay = TestData.CreateSchedule(indicator: StpIndicator.Override, service: permanent.Parent);
            
            var destination = permanent.Locations.Last().Location;

            var services = destination.FindExactArrivals(TestData.TenThirty);
            Assert.Equal(1, services.Length);
        }
        
        [Fact]
        public void LocationHasMultipleServicesForWhenOnSameTime()
        {
            var surbiton = TestLocations.Surbiton;
            var waterloo = TestLocations.WaterlooMain;
            
            var locations1 = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(surbiton, TestData.Ten),
                TestScheduleLocations.CreateDestination(waterloo, TestData.TenThirty)
            };            
            var service1 = TestData.CreateScheduleWithService(timetableId: "A00001", locations: locations1);
            
            var locations2 = new[]
            {
                (ScheduleLocation) TestScheduleLocations.CreateOrigin(surbiton, TestData.Ten),
                TestScheduleLocations.CreateDestination(waterloo, TestData.TenThirty)
            };             
            var service2 = TestData.CreateScheduleWithService(timetableId: "A00002", locations: locations2);
            
            var destination = service1.Locations.Last().Location;

            var services = destination.FindExactArrivals(TestData.TenThirty);
            Assert.Equal(2, services.Length);
        }
    }
}