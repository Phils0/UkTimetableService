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
            
            var services = new TimetableData();
            services.Add(service1);
            services.Add(service2);

            var schedule = services.GetSchedule(timetableUid, MondayAugust12);
            
            Assert.Equal(expected, schedule?.TimetableUid);
        }
        
        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Tuesday));
            
            var services = new TimetableData();           
            services.Add(schedule);
            services.Add(schedule2);

            var found = services.GetSchedule("X12345", MondayAugust12);
            Assert.Equal(schedule, found);
            
            found = services.GetSchedule("X12345", MondayAugust12.AddDays(1));
            Assert.Equal(schedule2, found);
            
            found = services.GetSchedule("X12345", MondayAugust12.AddDays(2));
            Assert.Null(found);
        }
    }
}