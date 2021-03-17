using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedAssociationStopTest
    {
        private ResolvedStop TestStop = new ResolvedStop(TestScheduleLocations.CreateStop(
            TestLocations.Surbiton,
            TestSchedules.Ten), DateTime.Today);
        
        [Fact]
        public void IsBrokenWhenNoStopInService()
        {
            var stop = new ResolvedAssociationStop(null, TestStop);
            Assert.True(stop.IsBroken);
        }
        
        [Fact]
        public void IsBrokenWhenNoStopInAssociatedService()
        {
            var stop = new ResolvedAssociationStop(TestStop, null);
            Assert.True(stop.IsBroken);            
        }
        
        [Fact]
        public void NotBrokenWHenHasBothStops()
        {
            var stop = new ResolvedAssociationStop(TestStop, TestStop);
            Assert.False(stop.IsBroken);           
        }
    }
}