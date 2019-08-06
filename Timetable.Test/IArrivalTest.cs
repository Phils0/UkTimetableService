using System;
using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class IArrivalTest
    {
        [Fact]
        public void NormalStopHasPublicArrival()
        {
            IArrival arrival = TestScheduleLocations.CreateStop(TestStations.Surbiton, TestSchedules.Ten);
            Assert.True(arrival.IsPublic);
        }
        
        [Fact]
        public void DestinationStopHasPublicArrival()
        {
            IArrival arrival = TestScheduleLocations.CreateDestination(TestStations.Surbiton, TestSchedules.Ten);
            Assert.True(arrival.IsPublic);
        }
        
        [Fact]
        public void PickupOnlyStopIsNotPublicArrival()
        {
            IArrival arrival = TestScheduleLocations.CreatePickupOnlyStop(TestStations.Surbiton, TestSchedules.Ten);
            Assert.False(arrival.IsPublic);
        }
        
        [Fact]
        public void SetdownStopHasPublicArrival()
        {
            IArrival arrival = TestScheduleLocations.CreateSetdownOnlyStop(TestStations.Surbiton, TestSchedules.Ten);
            Assert.True(arrival.IsPublic);
        }
    }
}