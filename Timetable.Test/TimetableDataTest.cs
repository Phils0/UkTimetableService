using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class TimetableDataTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        private static readonly DateTime TuesdayAugust13 =  MondayAugust12.AddDays(1);

        [Theory]
        [InlineData("A00001", "A00001")]
        [InlineData("A00002", "A00002")]
        [InlineData("A00003", null)]
        public void FindScheduleBasedUponTimetableUid(string timetableUid, string expected)
        {
            var timetable = CreateTimetable();
            
            TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A00001");
            TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A00002");
            
            var service = timetable.GetScheduleByTimetableUid(timetableUid, MondayAugust12).service;
            
            Assert.Equal(expected, service?.Details.TimetableUid);
        }

        private static TimetableData CreateTimetable()
        {
            return new TimetableData(Substitute.For<ILogger>());
        }

        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            
            var found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12);
            Assert.Equal(schedule, found.service.Details);
            Assert.Equal(MondayAugust12, found.service.On);

            found = timetable.GetScheduleByTimetableUid("X12345", TuesdayAugust13);
            Assert.Equal(schedule2, found.service.Details);
            Assert.Equal(TuesdayAugust13, found.service.On);
        }
        
        [Fact]
        public void ScheduleNotFound()
        {
            var timetable = CreateTimetable();           
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var found = timetable.GetScheduleByTimetableUid("Z98765", MondayAugust12);
            Assert.Null(found.service);
            Assert.Equal(LookupStatus.ServiceNotFound, found.status);
        }
        
        [Fact]
        public void ScheduleNotRunningOnDate()
        {
            var timetable = CreateTimetable();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
                       
            var found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12.AddDays(2));
            Assert.Null(found.service);
            Assert.Equal(LookupStatus.NoScheduleOnDate, found.status);
        }
        
        [Fact]
        public void ScheduleCancelledOnDate()
        {
            var timetable = CreateTimetable();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Weekdays));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            schedule2.StpIndicator = StpIndicator.Cancelled;
            
            var found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12);
            Assert.False(found.service.IsCancelled);
            Assert.Equal(schedule, found.service.Details);
            Assert.Equal(MondayAugust12, found.service.On);

            found = timetable.GetScheduleByTimetableUid("X12345", TuesdayAugust13);
            Assert.Equal(LookupStatus.Success, found.status);
            Assert.True(found.service.IsCancelled);
            Assert.Equal(TuesdayAugust13, found.service.On);
            Assert.Equal(schedule, found.service.Details);
        }
        
        [Theory]
        [InlineData("VT1000", "VT1000")]
        [InlineData("VT2000", "VT200000")]
        [InlineData("VT200000", "VT200000")]
        [InlineData("VT3000", null)]
        public void FindScheduleBasedUponRetailServiceId(string retailServiceId, string expected)
        {
            var timetable = CreateTimetable();
            
            TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A00001", retailServiceId: "VT1000");
            TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A00002", retailServiceId: "VT200000");

            var services = timetable.GetScheduleByRetailServiceId(retailServiceId, MondayAugust12);
            var service =  services.services.FirstOrDefault()?.Details;
            
            Assert.Equal(expected, service?.RetailServiceId);
        }
        
        [Fact]
        public void GetsScheduleRunningOnDateBasedUponRetailServiceId()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            
            var found = timetable.GetScheduleByRetailServiceId("VT123400", MondayAugust12).services.First();
            Assert.Equal(schedule, found.Details);
            Assert.Equal(MondayAugust12, found.On);

            found = timetable.GetScheduleByRetailServiceId("VT123400", TuesdayAugust13).services.First();
            Assert.Equal(TuesdayAugust13, found.On);
            Assert.Equal(schedule2, found.Details);
        }
        
        [Fact]
        public void ScheduleNotFoundBasedUponRetailServiceId()
        {
            var timetable = CreateTimetable();           
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var found = timetable.GetScheduleByRetailServiceId("VT999900", MondayAugust12);
            Assert.Empty(found.services);
            Assert.Equal(LookupStatus.ServiceNotFound, found.status);
        }
        
        [Theory]
        [InlineData("ABC")]
        [InlineData("")]
        public void GetScheduleByRetailServiceIdInvalidRetailServiceId(string retailServiceId)
        {
            var timetable = CreateTimetable();           
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var found = timetable.GetScheduleByRetailServiceId(retailServiceId, MondayAugust12);
            Assert.Empty(found.services);
            Assert.Equal(LookupStatus.InvalidRetailServiceId, found.status);
        }
        
        [Fact]
        public void ScheduleNotFoundBasedUponRetailServiceIdWhenHaveTwoDifferentRetailServiceIdsForSameService()
        {
            var timetable = CreateTimetable();           
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday), retailServiceId: "VT999900");
            
            var found = timetable.GetScheduleByRetailServiceId("VT999900", MondayAugust12);
            Assert.Empty(found.services);
            Assert.Equal(LookupStatus.NoScheduleOnDate, found.status);
        }
        
        [Fact]
        public void ScheduleNotRunningOnDateBasedUponRetailServiceId()
        {
            var timetable = CreateTimetable();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
                       
            var found = timetable.GetScheduleByRetailServiceId("VT123400", MondayAugust12.AddDays(2));
            Assert.Empty(found.services);
            Assert.Equal(LookupStatus.NoScheduleOnDate, found.status);
        }
        
        [Fact]
        public void ScheduleCancelledOnDateBasedUponRetailServiceId()
        {
            var timetable = CreateTimetable();           
            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Weekdays));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            schedule2.StpIndicator = StpIndicator.Cancelled;
            
            var found = timetable.GetScheduleByRetailServiceId("VT123400", MondayAugust12).services.First();
            Assert.False(found.IsCancelled);
            Assert.Equal(MondayAugust12, found.On);
            Assert.Equal(schedule, found.Details);
            
            found = timetable.GetScheduleByRetailServiceId("VT123400", TuesdayAugust13).services.First();
            Assert.True(found.IsCancelled);
            Assert.Equal(TuesdayAugust13, found.On);
            Assert.Equal(schedule, found.Details);
        }
        
        [Fact]
        public void GetsTocSchedulesRunningOnDate()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            var schedule3 = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "X98765", calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
           
            var found = timetable.GetSchedulesByToc("VT", MondayAugust12, Time.Midnight);
            var schedules = found.services.Select(s => s.Details).ToArray();
            Assert.Contains<Schedule>(schedule, schedules);
            Assert.Contains<Schedule>(schedule3, schedules);
            Assert.All(found.services, s => { Assert.Equal(MondayAugust12, s.On);});

            
            found = timetable.GetSchedulesByToc("VT", TuesdayAugust13, Time.Midnight);
            schedules = found.services.Select(s => s.Details).ToArray();
            Assert.Contains<Schedule>(schedule2, schedules);
            Assert.All(found.services, s => { Assert.Equal(TuesdayAugust13, s.On);});
        }
        
        [Fact]
        public void GetsTocSchedulesRunningOnRailDay()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            var startsRailDay = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "X98765", 
                calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday | DaysFlag.Tuesday),
                stops: TestSchedules.CreateThreeStopSchedule(Time.StartRailDay));
            var startsBeforeRailDay = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "X98764", 
                calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday | DaysFlag.Tuesday),
                stops: TestSchedules.CreateThreeStopSchedule(Time.StartRailDay.AddMinutes(-1)));
            var startsAfterRailDay = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "X98766", 
                calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday | DaysFlag.Tuesday),
                stops: TestSchedules.CreateThreeStopSchedule(Time.StartRailDay.AddMinutes(1)));
            
            var found = timetable.GetSchedulesByToc("VT", MondayAugust12, Time.StartRailDay);

            var startDaySchedule = found.services.Single(s => s.Details == startsRailDay);
            Assert.Equal(MondayAugust12, startDaySchedule.On);
            
            var startAfterDaySchedule = found.services.Single(s => s.Details == startsAfterRailDay);
            Assert.Equal(MondayAugust12, startAfterDaySchedule.On);

            var startBeforeDaySchedule = found.services.Single(s => s.Details == startsBeforeRailDay);
            Assert.Equal(TuesdayAugust13, startBeforeDaySchedule.On);
            
            Assert.Equal(4, found.services.Length);
        }
        
        [Fact]
        public void TocScheduleNotFound()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday));
            var schedule3 = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "X98765", calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
           
            var found = timetable.GetSchedulesByToc("GR", MondayAugust12, Time.Midnight);
            Assert.Empty(found.services);
            Assert.Equal(LookupStatus.ServiceNotFound, found.status);
        }
        
        [Fact]
        public void GetsCancelledSchedulesRunningOnDate()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Everyday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            schedule2.StpIndicator = StpIndicator.Cancelled;
           
            var found = timetable.GetSchedulesByToc("VT", MondayAugust12, Time.Midnight);
            var service = found.services[0];
            Assert.True(service.IsCancelled);
            Assert.Equal(MondayAugust12, service.On);
            Assert.Equal(schedule, service.Details);
        }
        
        [Fact]
        public void GetsTocSchedulesHandlesIndividualServiceError()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "CORRUPT", calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var schedule3 = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "X98765", calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));

            var data = timetable.AsDynamic()._timetableUidMap.RealObject as Dictionary<string, Service>;
            data["CORRUPT"] = null;    // Force an error
            
            var found = timetable.GetSchedulesByToc("VT", MondayAugust12, Time.Midnight);
            var schedules = found.services.Select(s => s.Details).ToArray();
            Assert.Contains<Schedule>(schedule, schedules);
            Assert.Contains<Schedule>(schedule3, schedules);
            Assert.All(found.services, s => { Assert.Equal(MondayAugust12, s.On);});
        }
    }
}