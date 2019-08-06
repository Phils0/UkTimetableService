using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleLocationIsStopAtTest
    {
        private static Time TenZeroOne => new Time(new TimeSpan(10, 1,0 ));

        [Fact]
        public void IsStopAtIfLocationMatchesAndPublicArrivalTimeMatches()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateStop(surbiton, TestSchedules.Ten);
            Assert.True(stop.IsStopAt(surbiton, TestSchedules.Ten));
        }
        
        [Fact]
        public void IsStopAtIfLocationMatchesAndPublicDepartureTimeMatches()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateStop(surbiton, TestSchedules.Ten);
            Assert.True(stop.IsStopAt(surbiton, TenZeroOne));
        }
        
        [Fact]
        public void IsStopAtIsFalseIfLocationMatchesAndArrivalTimeMatchesButIsPickupOnly()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreatePickupOnlyStop(surbiton, TestSchedules.TenThirty);
            stop.WorkingArrival = TestSchedules.Ten;
            Assert.False(stop.IsStopAt(surbiton, TestSchedules.Ten));
        }
        
        [Fact]
        public void IsStopAtIsFalseIfLocationMatchesAndDepartureTimeMatchesButIsSetdownOnly()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateSetdownOnlyStop(surbiton, TestSchedules.TenThirty);
            stop.WorkingDeparture = TenZeroOne;
            Assert.False(stop.IsStopAt(surbiton, TenZeroOne));
        }
        
        [Fact]
        public void IsStopAtIfLocationMatchesAndPublicArrivalTimeMatchesForDestination()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateDestination(surbiton, TestSchedules.Ten);
            Assert.True(stop.IsStopAt(surbiton, TestSchedules.Ten));
        }
        
        [Fact]
        public void IsStopAtIfLocationMatchesAndPublicDepartureTimeMatchesForOrigin()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateOrigin(surbiton, TestSchedules.Ten);
            Assert.True(stop.IsStopAt(surbiton, TestSchedules.Ten));
        }
    }
}