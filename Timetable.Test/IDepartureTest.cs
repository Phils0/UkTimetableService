using System;
using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class IDepartureTest
    {
        [Fact]
        public void NormalStopHasPublicDeparture()
        {
            IDeparture departure = TestScheduleLocations.CreateStop(TestStations.Surbiton, TestSchedules.Ten);
            Assert.True(departure.IsPublic);
        }
        
        [Fact]
        public void OriginStopHasPublicDeparture()
        {
            IDeparture departure = TestScheduleLocations.CreateOrigin(TestStations.Surbiton, TestSchedules.Ten);
            Assert.True(departure.IsPublic);
        }
        
        [Fact]
        public void PickupOnlyStopIsPublicDeparture()
        {
            IDeparture departure = TestScheduleLocations.CreatePickupOnlyStop(TestStations.Surbiton, TestSchedules.Ten);
            Assert.True(departure.IsPublic);
        }
        
        [Fact]
        public void SetdownStopHasIsNotPublicDeparture()
        {
            IDeparture departure = TestScheduleLocations.CreateSetdownOnlyStop(TestStations.Surbiton, TestSchedules.Ten);
            Assert.False(departure.IsPublic);
        }
    }
}