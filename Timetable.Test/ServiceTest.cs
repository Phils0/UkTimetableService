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
            var schedule = TestData.CreateSchedule();
            schedule.AddToService(service);

            Assert.Same(service, schedule.Parent);
        }
        
        [Fact]
        public void CanAddSchedulesWithDifferentStpIndicator()
        {
            var service = new Service("X12345");
            var permanent = TestData.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestData.EverydayAugust2019);
            var overlay = TestData.CreateSchedule(indicator: StpIndicator.Override, calendar: TestData.EverydayAugust2019);
            
            permanent.AddToService(service);
            overlay.AddToService(service);
            
            Assert.Same(service, permanent.Parent);
            Assert.Same(service, overlay.Parent);
        }
        
        // It happens we order by the calendar as need a unique order for the SortedList but could be anything that creates uniqueness
        [Fact]
        public void CanAddSchedulesWithSameStpIndicator()
        {
            var service = new Service("X12345");
            var permanent = TestData.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestData.EverydayAugust2019);
            var permanent2 = TestData.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestData.CreateAugust2019Calendar(DaysFlag.Monday));
            
            permanent.AddToService(service);
            permanent2.AddToService(service);
            
            Assert.Same(service, permanent.Parent);
            Assert.Same(service, permanent2.Parent);
        }
        
        [Fact]
        public void DoNotHaveOverlappingSchedulesWithTheSameStpIndicator()
        {
            //TODO
        }
        
        [Fact]
        public void CannotAddScheduleWithDifferentTimetableUid()
        {
            var schedule = TestData.CreateSchedule(timetableId: "A00002", indicator: StpIndicator.Permanent);
            
            var service = new Service("A00001");
            
            Assert.Throws<ArgumentException>(() => schedule.AddToService(service));
        }

        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule = TestData.CreateScheduleWithService(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Parent;
            
            var schedule2 = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Thursday), service: service);
            
            var found = service.GetScheduleOn(MondayAugust12.AddDays(2));
            Assert.Equal(schedule, found);
            
            found = service.GetScheduleOn(MondayAugust12.AddDays(3));
            Assert.Equal(schedule2, found);
        }
        
        [Fact]
        public void NullReturnedIfNoSchedulesRunOnDate()
        {
            var schedule = TestData.CreateScheduleWithService(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Parent;
            var schedule2 = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Thursday), service: service);
            
            var found = service.GetScheduleOn(MondayAugust12);
            Assert.Null(found);
        }
        
        [Theory]
        [InlineData(StpIndicator.Permanent, StpIndicator.Cancelled)]
        [InlineData(StpIndicator.Override, StpIndicator.Cancelled)]
        [InlineData(StpIndicator.New, StpIndicator.Cancelled)]
        [InlineData(StpIndicator.Permanent, StpIndicator.New)]
        [InlineData(StpIndicator.Override, StpIndicator.New)]
        [InlineData(StpIndicator.Permanent, StpIndicator.Override)]
        public void HighIndicatorsTakePriorityOverLow(StpIndicator lowIndicator, StpIndicator highIndicator)
        {
            var low = TestData.CreateScheduleWithService(indicator: lowIndicator, calendar: TestData.EverydayAugust2019);
            var service = low.Parent;
            var high = TestData.CreateSchedule(indicator: highIndicator, calendar: TestData.EverydayAugust2019, service: service);
            var low2 = TestData.CreateSchedule(indicator: lowIndicator, calendar: TestData.CreateAugust2019Calendar(DaysFlag.Monday), service: service);

            var found = service.GetScheduleOn(MondayAugust12);
            Assert.Equal(high, found);
        }
    }
}