using System;
using System.Linq;
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
            
            var service = timetable.GetScheduleByTimetableUid(timetableUid, MondayAugust12);
            
            Assert.Equal(expected, service.schedule?.TimetableUid);
        }
        
        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var timetable = new TimetableData();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            
            var found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12);
            Assert.Equal(schedule, found.schedule);
            
            found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12.AddDays(1));
            Assert.Equal(schedule2, found.schedule);
        }
        
        [Fact]
        public void ScheduleNotFound()
        {
            var timetable = new TimetableData();           
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var found = timetable.GetScheduleByTimetableUid("Z98765", MondayAugust12);
            Assert.Null(found.schedule);
            Assert.Equal(LookupStatus.ServiceNotFound, found.status);
        }
        
        [Fact]
        public void ScheduleNotRunningOnDate()
        {
            var timetable = new TimetableData();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
                       
            var found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12.AddDays(2));
            Assert.Null(found.schedule);
            Assert.Equal(LookupStatus.NoScheduleOnDate, found.status);
        }
        
        [Fact]
        public void ScheduleCancelledOnDate()
        {
            var timetable = new TimetableData();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Weekdays));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            schedule2.StpIndicator = StpIndicator.Cancelled;
            
            var found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12);
            Assert.Equal(schedule, found.schedule);
            
            found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12.AddDays(1));
            Assert.Null(found.schedule);
            Assert.Equal(LookupStatus.CancelledService, found.status);
        }
        
        [Theory]
        [InlineData("VT1000", "VT1000")]
        [InlineData("VT2000", "VT2000")]
        [InlineData("VT3000", null)]
        public void FindScheduleBasedUponRetailServiceId(string retailServiceId, string expected)
        {
            var timetable = new TimetableData();
            
            TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A00001", retailServiceId: "VT1000");
            TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A00002", retailServiceId: "VT2000");

            var services = timetable.GetScheduleByRetailServiceId(retailServiceId, MondayAugust12);
            var service =  services.schedule.FirstOrDefault();
            
            Assert.Equal(expected, service?.RetailServiceId);
        }
        
        [Fact]
        public void GetsScheduleRunningOnDateBasedUponRetailServiceId()
        {
            var timetable = new TimetableData();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            
            var found = timetable.GetScheduleByRetailServiceId("VT123400", MondayAugust12);
            var foundSchedule =  found.schedule.First();
            Assert.Equal(schedule, foundSchedule);
            
            found = timetable.GetScheduleByRetailServiceId("VT123400", MondayAugust12.AddDays(1));
            foundSchedule =  found.schedule.First();
            Assert.Equal(schedule2, foundSchedule);
        }
        
        [Fact]
        public void ScheduleNotFoundBasedUponRetailServiceId()
        {
            var timetable = new TimetableData();           
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var found = timetable.GetScheduleByRetailServiceId("VT999900", MondayAugust12);
            Assert.Empty(found.schedule);
            Assert.Equal(LookupStatus.ServiceNotFound, found.status);
        }
        
        [Fact]
        public void ScheduleNotFoundBasedUponRetailServiceIdWhenHaveTwoDifferentRetailServiceIdsForSameService()
        {
            var timetable = new TimetableData();           
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday), retailServiceId: "VT999900");
            
            var found = timetable.GetScheduleByRetailServiceId("VT999900", MondayAugust12);
            Assert.Empty(found.schedule);
            Assert.Equal(LookupStatus.NoScheduleOnDate, found.status);
        }
        
        [Fact]
        public void ScheduleNotRunningOnDateBasedUponRetailServiceId()
        {
            var timetable = new TimetableData();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
                       
            var found = timetable.GetScheduleByRetailServiceId("VT123400", MondayAugust12.AddDays(2));
            Assert.Empty(found.schedule);
            Assert.Equal(LookupStatus.NoScheduleOnDate, found.status);
        }
        
        [Fact]
        public void ScheduleCancelledOnDateBasedUponRetailServiceId()
        {
            var timetable = new TimetableData();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Weekdays));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            schedule2.StpIndicator = StpIndicator.Cancelled;
            
            var found = timetable.GetScheduleByRetailServiceId("VT123400", MondayAugust12);
            var foundSchedule =  found.schedule.First();
            Assert.Equal(schedule, foundSchedule);
            
            found = timetable.GetScheduleByRetailServiceId("VT123400", MondayAugust12.AddDays(1));
            Assert.Empty(found.schedule);
            Assert.Equal(LookupStatus.CancelledService, found.status);
        }
        
        [Fact]
        public void GetsTocSchedulesRunningOnDate()
        {
            var timetable = new TimetableData();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            var schedule3 = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "X98765", calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
           
            var found = timetable.GetSchedulesByToc("VT", MondayAugust12);
            Assert.Contains<Schedule>(schedule, found.schedules);
            Assert.Contains<Schedule>(schedule3, found.schedules);

            
            found = timetable.GetSchedulesByToc("VT", MondayAugust12.AddDays(1));
            Assert.Contains<Schedule>(schedule2, found.schedules);
        }
        
        [Fact]
        public void TocScheduleNotFounds()
        {
            var timetable = new TimetableData();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            var schedule3 = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "X98765", calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
           
            var found = timetable.GetSchedulesByToc("GR", MondayAugust12);
            Assert.Empty(found.schedules);
            Assert.Equal(LookupStatus.ServiceNotFound, found.status);
        }
    }
}