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
            var service1 = TestData.CreateSchedule(id: "A00001");
            var service2 = TestData.CreateSchedule(id: "A00002");
            
            var timetable = new TimetableData();
            timetable.Add(service1);
            timetable.Add(service2);

            var service = timetable.GetSchedule(timetableUid, MondayAugust12);
            
            Assert.Equal(expected, service.schedule?.TimetableUid);
        }
        
        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Tuesday));
            
            var timetable = new TimetableData();           
            timetable.Add(schedule);
            timetable.Add(schedule2);

            var found = timetable.GetSchedule("X12345", MondayAugust12);
            Assert.Equal(schedule, found.schedule);
            
            found = timetable.GetSchedule("X12345", MondayAugust12.AddDays(1));
            Assert.Equal(schedule2, found.schedule);
        }
        
        [Fact]
        public void ScheduleNotFound()
        {
            var schedule = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var timetable = new TimetableData();           
            timetable.Add(schedule);
            
            var found = timetable.GetSchedule("Z98765", MondayAugust12);
            Assert.Null(found.schedule);
            Assert.Equal("Z98765 not found in timetable", found.reason);
        }
        
        [Fact]
        public void ScheduleNotRunningOnDate()
        {
            var schedule = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Tuesday));
            
            var timetable = new TimetableData();           
            timetable.Add(schedule);
            timetable.Add(schedule2);
            
            var found = timetable.GetSchedule("X12345", MondayAugust12.AddDays(2));
            Assert.Null(found.schedule);
            Assert.Equal("X12345 does not run on 14/08/2019", found.reason);
        }
        
        [Fact]
        public void ScheduleCancelledOnDate()
        {
            var schedule = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Weekdays));
            var schedule2 = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Tuesday));
            schedule2.StpIndicator = StpIndicator.Cancelled;
            
            var timetable = new TimetableData();           
            timetable.Add(schedule);
            timetable.Add(schedule2);

            var found = timetable.GetSchedule("X12345", MondayAugust12);
            Assert.Equal(schedule, found.schedule);
            
            found = timetable.GetSchedule("X12345", MondayAugust12.AddDays(1));
            Assert.Null(found.schedule);
            Assert.Equal("X12345 cancelled in STP on 13/08/2019", found.reason);
        }
    }
}