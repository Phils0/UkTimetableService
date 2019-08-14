using System;
using Timetable.Test.Data;
using Xunit;
using Xunit.Sdk;

namespace Timetable.Test
{
    public class ServiceTest
    {
        [Fact]
        public void ParentSetToService()
        {
            var service = new Service("X12345");
            var schedule = TestSchedules.CreateSchedule();
            schedule.AddToService(service);

            Assert.Same(service, schedule.Service);
        }
        
        [Fact]
        public void CanAddSchedulesWithDifferentStpIndicator()
        {
            var service = new Service("X12345");
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
            var overlay = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, calendar: TestSchedules.EverydayAugust2019);
            
            permanent.AddToService(service);
            overlay.AddToService(service);
            
            Assert.Same(service, permanent.Service);
            Assert.Same(service, overlay.Service);
        }
        
        // It happens we order by the calendar as need a unique order for the SortedList but could be anything that creates uniqueness
        [Fact]
        public void CanAddSchedulesWithSameStpIndicator()
        {
            var service = new Service("X12345");
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
            var permanent2 = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            permanent.AddToService(service);
            permanent2.AddToService(service);
            
            Assert.Same(service, permanent.Service);
            Assert.Same(service, permanent2.Service);
        }
        
        [Fact]
        public void DoNotHaveOverlappingSchedulesWithTheSameStpIndicator()
        {
            //TODO
        }
        
        [Fact]
        public void CannotAddScheduleWithDifferentTimetableUid()
        {
            var schedule = TestSchedules.CreateSchedule(timetableId: "A00002", indicator: StpIndicator.Permanent);
            
            var service = new Service("A00001");
            
            Assert.Throws<ArgumentException>(() => schedule.AddToService(service));
        }

        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            
            var schedule2 = TestSchedules.CreateSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Thursday), service: service);
            
            var found = service.GetScheduleOn(MondayAugust12.AddDays(2));
            Assert.Equal(schedule, found.Details);
            
            found = service.GetScheduleOn(MondayAugust12.AddDays(3));
            Assert.Equal(schedule2, found.Details);
        }
        
        [Fact]
        public void NullReturnedIfNoSchedulesRunOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            var schedule2 = TestSchedules.CreateSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Thursday), service: service);
            
            var found = service.GetScheduleOn(MondayAugust12);
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

            var found = service.GetScheduleOn(MondayAugust12);
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

            var found = service.GetScheduleOn(MondayAugust12);
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

            var found = service.GetScheduleOn(MondayAugust12);
            Assert.True(found.IsCancelled);
            Assert.Equal(overrideSchedule,found.Details);
        }
        
        [Fact]
        public void TryFindScheduleOnRunningOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            
            Assert.True(service.TryFindScheduleOn(MondayAugust12.AddDays(2), out var found));
            Assert.Equal(schedule, found.Details);
        }
        
        [Fact]
        public void TryFindScheduleReturnsFalseWhenNoSchedulesRunOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;

            Assert.False(service.TryFindScheduleOn(MondayAugust12, out var found));
            Assert.Null(found);
        }
        
        [Fact]
        public void TryFindScheduleStopOnRunningOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            var find = new StopSpecification(TestStations.ClaphamJunction, TestSchedules.TenFifteen, MondayAugust12.AddDays(2));
            
            Assert.True(service.TryFindScheduledStopOn(find, out var found));
            Assert.Equal(schedule, found.Details);
        }
        
        [Fact]
        public void TryFindScheduleStopReturnsFalseWhenNoSchedulesRunOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            var find = new StopSpecification(TestStations.ClaphamJunction, TestSchedules.TenFifteen, MondayAugust12);
          
            Assert.False(service.TryFindScheduledStopOn(find, out var found));
            Assert.Null(found);
        }
        
        [Fact]
        public void TryFindScheduleStopReturnsFalseWhenNoStopAtTime()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            var find = new StopSpecification(TestStations.ClaphamJunction, TestSchedules.Ten, MondayAugust12.AddDays(2));
            
            Assert.False(service.TryFindScheduledStopOn(find, out var found));
            Assert.Null(found);
        }
    }
}