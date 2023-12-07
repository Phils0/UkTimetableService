using System;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class TocServicesFilterTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        
        private static TimetableData CreateTimetable()
        {
            var logger = Substitute.For<ILogger>();
            return new TimetableData(new ServiceFilters(false, logger), logger);
        }
        
        [Fact]
        public void OnlyReturnsTocServices()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, retailServiceId: "VT123400");
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "C98765", retailServiceId: "XC987600");

            var filter = new TocServicesFilter(timetable, Filters);
            var found = filter.GetServicesByToc("VT", MondayAugust12, Time.Midnight, false);

            Assert.All(found.services, s => Assert.True(s.OperatedBy("VT")));
        }
        
        [Fact]
        public void DoesNotReturnCancelledSchedulesRunningOnDate()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Everyday));
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            schedule2.StpIndicator = StpIndicator.Cancelled;

            var filter = new TocServicesFilter(timetable, Filters);
            var found = filter.GetServicesByToc("VT", MondayAugust12, Time.Midnight, false);
            
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
           
            var filter = new TocServicesFilter(timetable, Filters);
            var found = filter.GetServicesByToc("VT", MondayAugust12, Time.Midnight, true);
            
            var service = found.services[0];
            Assert.True(service.IsCancelled);
            Assert.Equal(MondayAugust12, service.On);
            Assert.Equal(schedule, service.Details);
        }

        [Fact]
        public void OnlyReturnsTrainIdentityServices()
        {
            var timetable = CreateTimetable();           

            var schedule = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A12345");
            var schedule2 = TestSchedules.CreateScheduleInTimetable(timetable, timetableId: "A98765");
            schedule2.StpIndicator = StpIndicator.Cancelled;

            var filter = new TocServicesFilter(timetable, Filters);
            var found = filter.GetServicesByTrainIdentity("VT", MondayAugust12, "9Z12", Time.Midnight, false);

            var service = found.services[0];
            Assert.Equal(MondayAugust12, service.On);
            Assert.Equal(schedule, service.Details);
        }
    }
}