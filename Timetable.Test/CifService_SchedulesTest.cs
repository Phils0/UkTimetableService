using System;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class CifService_SchedulesTest
    {
        [Fact]
        public void CanAddSchedulesWithDifferentStpIndicator()
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
            var overlay = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, calendar: TestSchedules.EverydayAugust2019);
            
            service.Add(permanent);
            service.Add(overlay);

            var schedules = service.GetSchedules();
            Assert.Equal(2, schedules.Count);
        }
        

        // It happens we order by the calendar as need a unique order for the SortedList but could be anything that creates uniqueness
        [Fact]
        public void CanAddSchedulesWithSameStpIndicator()
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
            var permanent2 = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            service.Add(permanent);
            service.Add(permanent2);
            
            var schedules = service.GetSchedules();
            Assert.Equal(2, schedules.Count);
        }
        
        [Fact]
        public void CannotAddSameSchedulesTwice()
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            service.Add(permanent);
            var ex = Assert.Throws<ArgumentException>(() =>  service.Add(permanent));
            
            Assert.StartsWith("Schedule already added", ex.Message);
        }
        
        [Fact]
        public void DoNotHaveOverlappingSchedulesWithTheSameStpIndicator()
        {
            //TODO
        }
        
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            
            var schedule2 = TestSchedules.CreateSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Thursday), service: service);
            
            service.TryResolveOn(MondayAugust12.AddDays(2), out var found);
            Assert.Equal(schedule, found.Details);
            
            service.TryResolveOn(MondayAugust12.AddDays(3), out found);
            Assert.Equal(schedule2, found.Details);
        }
        
        [Fact]
        public void NullReturnedIfNoSchedulesRunOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            var schedule2 = TestSchedules.CreateSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Thursday), service: service);
            
            service.TryResolveOn(MondayAugust12, out var found);
            Assert.Null(found);
        }
        
        [Theory]
        [InlineData(StpIndicator.Permanent, StpIndicator.New)]
        [InlineData(StpIndicator.Override, StpIndicator.New)]
        [InlineData(StpIndicator.Permanent, StpIndicator.Override)]
        public void HighIndicatorsTakePriorityOverLow(StpIndicator lowIndicator, StpIndicator highIndicator)
        {
            var low = TestSchedules.CreateScheduleWithService(indicator: lowIndicator, calendar: TestSchedules.EverydayAugust2019);
            var service = low.Service;
            var high = TestSchedules.CreateSchedule(indicator: highIndicator, calendar: TestSchedules.EverydayAugust2019, service: service);
            TestSchedules.CreateSchedule(indicator: lowIndicator, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);

            service.TryResolveOn(MondayAugust12, out var found);
            Assert.Equal(high, found.Details);
        }
        
        [Theory]
        [InlineData(StpIndicator.Permanent)]
        [InlineData(StpIndicator.Override)]
        [InlineData(StpIndicator.New)]
        public void CancelledScheduleReturned(StpIndicator lowIndicator)
        {
            var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: lowIndicator, calendar: TestSchedules.EverydayAugust2019);
            var service = baseSchedule.Service;
            TestSchedules.CreateSchedule(indicator: StpIndicator.Cancelled, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);

            service.TryResolveOn(MondayAugust12, out var found);
            Assert.True(found.IsCancelled);
            Assert.Equal(baseSchedule,found.Details);
        }
        
        [Fact]
        public void MultipleScheduleRecordsWithCancelReturnsHighestPriority()
        {
            var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
            var service = baseSchedule.Service;
            TestSchedules.CreateSchedule(indicator: StpIndicator.Cancelled, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);
            var overrideSchedule = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);

            service.TryResolveOn(MondayAugust12, out var found);
            Assert.True(found.IsCancelled);
            Assert.Equal(overrideSchedule,found.Details);
        }
        
        [Fact]
        public void TryFindScheduleOnRunningOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            
            Assert.True(service.TryResolveOn(MondayAugust12.AddDays(2), out var found));
            Assert.Equal(schedule, found.Details);
        }
        
        [Fact]
        public void TryFindScheduleReturnsFalseWhenNoSchedulesRunOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;

            Assert.False(service.TryResolveOn(MondayAugust12, out var found));
            Assert.Null(found);
        }
        
        [Fact]
        public void TryFindScheduleStopOnRunningOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            var find = CreateFindSpec(TestSchedules.TenSixteen, MondayAugust12.AddDays(2));
            
            Assert.True(service.TryFindScheduledStop(find, out var found));
            Assert.Equal(schedule, found.Service.Details);
        }

        private StopSpecification CreateFindSpec(Time time, DateTime onDate)
        {
            return new StopSpecification(TestStations.ClaphamJunction, time, onDate, TimesToUse.Departures);
        }

        [Fact]
        public void TryFindScheduleStopReturnsFalseWhenNoSchedulesRunOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            var find = CreateFindSpec(TestSchedules.TenFifteen, MondayAugust12);
          
            Assert.False(service.TryFindScheduledStop(find, out var found));
            Assert.Null(found);
        }
        
        [Fact]
        public void TryFindScheduleStopReturnsFalseWhenNoStopAtTime()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            var find = CreateFindSpec(TestSchedules.Ten, MondayAugust12.AddDays(2));
            
            Assert.False(service.TryFindScheduledStop(find, out var found));
            Assert.Null(found);
        }
        
        private static readonly Time TwentyThreeFiftyFive = new Time(new TimeSpan(23, 55, 0));
        private static readonly Time MidnightEleven = new Time(new TimeSpan(0, 11, 0));
       
        [Fact]
        public void TryFindScheduleStopOnFindsStopOnNextDay()
        {
            var schedule = CreateServiceStartingAt(TwentyThreeFiftyFive);
            var find = CreateFindSpec(MidnightEleven, MondayAugust12);
            
            Assert.True(schedule.Service.TryFindScheduledStop(find, out var found));
            Assert.Equal(schedule, found.Service.Details);
            Assert.Equal(MondayAugust12.AddDays(-1), found.On);
        }

        private CifSchedule CreateServiceStartingAt(Time startsAt)
        {
            var stops = TestSchedules.CreateThreeStopSchedule(startsAt);
            return TestSchedules.CreateScheduleWithService(stops: stops);
        }

        public static TheoryData<Time, Time, bool> StartsBeforeData =>
            new TheoryData<Time, Time, bool>()
            {
                {Time.Midnight, Time.Midnight, false },
                {Time.Midnight.AddMinutes(1), Time.Midnight, false },
                {new Time(new TimeSpan(23, 59, 0)), Time.Midnight, false },
                {Time.Midnight.AddDay().AddMinutes(1), Time.Midnight, false },
                {Time.StartRailDay, Time.StartRailDay, false },
                {Time.StartRailDay.AddMinutes(1), Time.StartRailDay, false },
                {Time.StartRailDay.AddMinutes(-1), Time.StartRailDay, true },
                {Time.StartRailDay.AddDay(), Time.StartRailDay, false },
                {Time.StartRailDay.AddDay().AddMinutes(1), Time.StartRailDay, false },
                {Time.StartRailDay.AddDay().AddMinutes(-1), Time.StartRailDay, true },
            };
        
        [Theory]
        [MemberData(nameof(StartsBeforeData))]
        public void StartsBefore(Time serviceStart, Time boundary, bool expected)
        {
            var service = CreateServiceStartingAt(serviceStart).Service;
            Assert.Equal(expected, service.StartsBefore(boundary));        
        }

        [Fact]
        public void StartsBeforeSkipsCancelledServices()
        {
            var service = CreateServiceStartingAt(Time.StartRailDay.AddMinutes(-1)).Service;
            TestSchedules.CreateSchedule(indicator: StpIndicator.Cancelled, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);
            
            Assert.True(service.StartsBefore(Time.StartRailDay));   
        }

        // This happens occasionally when get dodgy services in the cif file 
        [Fact]
        public void StartsBeforeThrowsInvalidOperationExceptionWhenNoDepartures()
        {
            var start = TestSchedules.Ten;
            var stops = new ScheduleLocation[]
            {
                TestScheduleLocations.CreatePass(TestStations.Vauxhall, start.AddMinutes(10)),
                TestScheduleLocations.CreatePass(TestStations.ClaphamJunction, start.AddMinutes(20)),
                TestScheduleLocations.CreatePass(TestStations.Wimbledon, start.AddMinutes(20)),
            };
            var service =  TestSchedules.CreateScheduleWithService(stops: stops).Service;
            
            Assert.Throws<InvalidOperationException>(() => service.StartsBefore(Time.StartRailDay));   
        }
        
        [Fact]
        public void TryGetScheduleNoSchedules()
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            
            Assert.False(service.TryGetSchedule((StpIndicator.Permanent, TestSchedules.EverydayAugust2019), out var schedule));
            Assert.Null(schedule);
        }
        
        [Fact]
        public void TryGetScheduleSingleSchedule()
        {
            var baseSchedule = TestSchedules.CreateScheduleWithService();
            var service = baseSchedule.Service;
            
            Assert.True(service.TryGetSchedule((StpIndicator.Permanent, TestSchedules.EverydayAugust2019), out var schedule));
            Assert.Equal(baseSchedule, schedule);
        }
        
        public static TheoryData<StpIndicator, bool, bool> StpTests =>
            new TheoryData<StpIndicator, bool, bool>()
            {
                {StpIndicator.Permanent, true, true },
                {StpIndicator.Override, true, false },
                {StpIndicator.New, false, false },
            };
        
        [Theory]
        [MemberData(nameof(StpTests))]
        public void TryGetScheduleMultipleSchedulesDifferentStp(StpIndicator indicator, bool expectedFound, bool isPermanent)
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
            var overlay = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, calendar: TestSchedules.EverydayAugust2019);
            
            service.Add(permanent);
            service.Add(overlay);
            
            Assert.Equal(expectedFound, service.TryGetSchedule((indicator, TestSchedules.EverydayAugust2019), out var schedule));
            if (expectedFound)
            {
                var expected = isPermanent ? permanent : overlay;
                Assert.Equal(expected, schedule);
            }
        }
        
        public static TheoryData<ICalendar, bool, bool> CalendarTests =>
            new TheoryData<ICalendar, bool, bool>()
            {
                {TestSchedules.EverydayAugust2019, true, true },
                {TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), true, false },
                {TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday), false, false },
            };
        
        [Theory]
        [MemberData(nameof(CalendarTests))]
        public void TryGetScheduleMultipleSchedulesDifferentCalendar(ICalendar calendar, bool expectedFound, bool isFirst)
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
            var permanent2 = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            service.Add(permanent);
            service.Add(permanent2);
            
            Assert.Equal(expectedFound,service.TryGetSchedule((StpIndicator.Permanent, calendar), out var schedule));
            if (expectedFound)
            {
                var expected = isFirst ? permanent : permanent2;
                Assert.Equal(expected, schedule);
            }
        }
    }
}