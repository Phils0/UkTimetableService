using System;
using NSubstitute;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleGathererTest
    {
        private static readonly DateTime TestDate = new DateTime(2019, 8, 9);

        [Fact]
        public void GatherSingleServiceAfter()
        {
            var schedule = CreateStubSchedule();

            var gatherer = new ScheduleGatherer(schedule);
            var services = gatherer.Gather(5, 0, 1, TestDate);

            Assert.Single(services);
        }

        private static ISchedule CreateStubSchedule()
        {
            var schedule = Substitute.For<ISchedule>();
            schedule.Location.Returns(TestStations.Surbiton);
            schedule.Count.Returns(10);
            schedule.ValuesAt(5).Returns((TestSchedules.Ten, new[] {TestSchedules.CreateScheduleWithService().Service}));
            return schedule;
        }
    }
}