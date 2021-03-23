using System;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class FilterServicesDecoratorTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        
        private static TimetableData CreateTimetable()
        {
            var logger = Substitute.For<ILogger>();
            return new TimetableData(new ServiceFilters(false, logger), logger);
        }
        
        [Fact]
        public void DoesNotReturnCancelledSchedulesRunningOnDate()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Everyday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            schedule2.StpIndicator = StpIndicator.Cancelled;

            var decorator = new FilterServicesDecorator(timetable, Filters);
            var found = decorator.GetServicesByToc(false)("VT", MondayAugust12, Time.Midnight);
            Assert.Empty(found.services);
        }

        private ServiceFilters Filters => new ServiceFilters(false, Substitute.For<ILogger>());

        [Fact]
        public void GetsCancelledSchedulesRunningOnDate()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Everyday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            schedule2.StpIndicator = StpIndicator.Cancelled;
           
            var decorator = new FilterServicesDecorator(timetable, Filters);
            var found = decorator.GetServicesByToc(true)("VT", MondayAugust12, Time.Midnight);
            var service = found.services[0];
            Assert.True(service.IsCancelled);
            Assert.Equal(MondayAugust12, service.On);
            Assert.Equal(schedule, service.Details);
        }

    }
}