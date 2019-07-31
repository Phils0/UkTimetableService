using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Theory]
        [InlineData("VT123400", true)]
        [InlineData("VT999900", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsRetailServiceId(string retailServiceId, bool expected)
        {
            var schedule = TestSchedules.CreateSchedule();
            Assert.Equal(expected, schedule.HasRetailServiceId(retailServiceId));
        }
        
        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule = TestSchedules.CreateSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            Assert.True(schedule.RunsOn(MondayAugust12));
            Assert.False(schedule.RunsOn(MondayAugust12.AddDays(1)));
        }
        
    }
}