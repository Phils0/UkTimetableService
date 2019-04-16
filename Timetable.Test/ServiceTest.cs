using System;
using Timetable.Test.Data;
using Xunit;
using Xunit.Sdk;

namespace Timetable.Test
{
    public class ServiceTest
    {
        // It happens we order by the calendar as need a unique order for the SortedList but could be anything that creates uniqueness
        [Fact]
        public void CanAddSchedulesWithSameStpIndicator()
        {
            var permanent = TestData.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestData.EverydayAugust2019);
            var permanent2 = TestData.CreateSchedule(indicator: StpIndicator.Permanent, calendar: TestData.CreateAugust2019Calendar(DaysFlag.Monday));

            var service = new Service(permanent);
            service.Add(permanent2);
        }
        
        [Fact]
        public void DoNotHaveOverlappingSchedulesWithTheSameStpIndicator()
        {
            //TODO
        }
        
        [Fact]
        public void CannotAddTwoSchedulesWithDifferentTimetableUidsToAService()
        {
            var permanent = TestData.CreateSchedule(id: "A00001", indicator: StpIndicator.Permanent);
            var overrideOverlay = TestData.CreateSchedule(id: "A00002", indicator: StpIndicator.Override);

            var service = new Service(permanent);
            
            Assert.Throws<ArgumentException>(() => service.Add(overrideOverlay));
        }

        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var schedule2 = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Thursday));
            
            var service = new Service(schedule);           
            service.Add(schedule2);

            var found = service.GetScheduleOn(MondayAugust12.AddDays(2));
            Assert.Equal(schedule, found);
            
            found = service.GetScheduleOn(MondayAugust12.AddDays(3));
            Assert.Equal(schedule2, found);
        }
        
        [Fact]
        public void NullReturnedIfNoSchedulesRunOnDate()
        {
            var schedule = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var schedule2 = TestData.CreateSchedule(calendar: TestData.CreateAugust2019Calendar(DaysFlag.Thursday));
            
            var service = new Service(schedule);           
            service.Add(schedule2);

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
            var low = TestData.CreateSchedule(indicator: lowIndicator, calendar: TestData.EverydayAugust2019);
            var high = TestData.CreateSchedule(indicator: highIndicator, calendar: TestData.EverydayAugust2019);
            var low2 = TestData.CreateSchedule(indicator: lowIndicator, calendar: TestData.CreateAugust2019Calendar(DaysFlag.Monday));

            var service = new Service(low);
            service.Add(high);           
            service.Add(low2);

            var found = service.GetScheduleOn(MondayAugust12);
            Assert.Equal(high, found);
        }
    }
}