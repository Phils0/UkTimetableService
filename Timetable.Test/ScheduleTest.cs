using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleTest
    {
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
        
    }
}