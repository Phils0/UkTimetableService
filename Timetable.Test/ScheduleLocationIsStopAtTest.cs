using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleLocationIsStopAtTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        private static Time TenZeroOne => new Time(new TimeSpan(10, 1,0 ));

        [Fact]
        public void IsStopAtIfLocationMatchesAndPublicArrivalTimeMatches()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateStop(surbiton, TestSchedules.Ten);
            var spec = new StopSpecification(surbiton, TestSchedules.Ten, MondayAugust12);
            Assert.True(stop.IsStopAt(spec));
        }

        [Fact]
        public void IsStopAtIfLocationMatchesAndPublicDepartureTimeMatches()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateStop(surbiton, TestSchedules.Ten);
            var spec = new StopSpecification(surbiton, TenZeroOne, MondayAugust12);
            Assert.True(stop.IsStopAt(spec));
        }

        [Fact]
        public void IsStopAtIsFalseIfLocationMatchesAndArrivalTimeMatchesButIsPickupOnly()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreatePickupOnlyStop(surbiton, TestSchedules.TenThirty);
            stop.WorkingArrival = TestSchedules.Ten;
            var spec = new StopSpecification(surbiton, TestSchedules.Ten, MondayAugust12);
            Assert.False(stop.IsStopAt(spec));
        }
        
        [Fact]
        public void IsStopAtIsFalseIfLocationMatchesAndDepartureTimeMatchesButIsSetdownOnly()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateSetdownOnlyStop(surbiton, TestSchedules.TenThirty);
            stop.WorkingDeparture = TenZeroOne;
            var spec = new StopSpecification(surbiton, TenZeroOne, MondayAugust12);
            Assert.False(stop.IsStopAt(spec));
        }
        
        [Fact]
        public void IsStopAtIfLocationMatchesAndPublicArrivalTimeMatchesForDestination()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateDestination(surbiton, TestSchedules.Ten);
            var spec = new StopSpecification(surbiton, TestSchedules.Ten, MondayAugust12);
            Assert.True(stop.IsStopAt(spec));
        }
        
        [Fact]
        public void IsStopAtIfLocationMatchesAndPublicDepartureTimeMatchesForOrigin()
        {
            var surbiton = TestStations.Surbiton;
            var stop = TestScheduleLocations.CreateOrigin(surbiton, TestSchedules.Ten);
            var spec = new StopSpecification(surbiton, TestSchedules.Ten, MondayAugust12);
            Assert.True(stop.IsStopAt(spec));
        }
    }
}