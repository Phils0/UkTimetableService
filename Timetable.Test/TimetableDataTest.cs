using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class TimetableDataTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Theory]
        [InlineData("A00001", "A00001")]
        [InlineData("A00002", "A00002")]
        [InlineData("A00003", null)]
        public void FindScheduleBasedUponTimetableUid(string timetableUid, string expected)
        {
            var timetable = new TimetableData();
            
            TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A00001");
            TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A00002");
            
            var service = timetable.GetSchedule(timetableUid, MondayAugust12);
            
            Assert.Equal(expected, service.schedule?.TimetableUid);
        }
        
        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var timetable = new TimetableData();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            
            var found = timetable.GetSchedule("X12345", MondayAugust12);
            Assert.Equal(schedule, found.schedule);
            
            found = timetable.GetSchedule("X12345", MondayAugust12.AddDays(1));
            Assert.Equal(schedule2, found.schedule);
        }
        
        [Fact]
        public void ScheduleNotFound()
        {
            var timetable = new TimetableData();           
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var found = timetable.GetSchedule("Z98765", MondayAugust12);
            Assert.Null(found.schedule);
            Assert.Equal("Z98765 not found in timetable", found.reason);
        }
        
        [Fact]
        public void ScheduleNotRunningOnDate()
        {
            var timetable = new TimetableData();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
                       
            var found = timetable.GetSchedule("X12345", MondayAugust12.AddDays(2));
            Assert.Null(found.schedule);
            Assert.Equal("X12345 does not run on 14/08/2019", found.reason);
        }
        
        [Fact]
        public void ScheduleCancelledOnDate()
        {
            var timetable = new TimetableData();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Weekdays));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            schedule2.StpIndicator = StpIndicator.Cancelled;
            
            var found = timetable.GetSchedule("X12345", MondayAugust12);
            Assert.Equal(schedule, found.schedule);
            
            found = timetable.GetSchedule("X12345", MondayAugust12.AddDays(1));
            Assert.Null(found.schedule);
            Assert.Equal("X12345 cancelled in STP on 13/08/2019", found.reason);
        }
    }
}