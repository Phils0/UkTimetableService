using System;
using System.Linq;
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
            var schedule = CreateMockSchedule();
            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneService);
            var services = gatherer.Gather(2, TestDate);

            Assert.Single(services);
            var stop = services[0].Stop as ScheduleOrigin;
            Assert.Equal(TestStations.Surbiton, stop.Station);
            Assert.Equal(TestSchedules.TenFifteen, stop.Departure);
        }

        private ISchedule CreateMockSchedule((int idx, (Time, Service[]) services)[] responses = null)
        {
            responses = responses ?? (new []
            {
                (idx: 0, services: CreateTimeEntry(TestSchedules.Ten.AddMinutes(-60))), 
                (idx: 1, services: CreateTimeEntry(TestSchedules.Ten)), 
                (idx: 2, services: CreateTimeEntry(TestSchedules.TenFifteen)),
                (idx: 3, services: CreateTimeEntry(TestSchedules.TenThirty))
            });

            var maxIdx = responses.Max(r => r.idx);
            
            var schedule = Substitute.For<ISchedule>();
            schedule.Location.Returns(TestStations.Surbiton);
            schedule.Count.Returns(maxIdx + 1);
            foreach (var tuple in responses)
            {
                schedule.ValuesAt(tuple.idx).Returns(tuple.services);
            }

            return schedule;
        }

        [Fact]
        public void GatherMultipleServiceAfter()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.Create(0, 2));
            var services = gatherer.Gather(1, TestDate);

            Assert.Equal(2, services.Length);
        }

        private (Time, Service[]) CreateTimeEntry(Time time)
        {
            return (time, new[]
            {
                TestSchedules.CreateScheduleWithService(locations: TestSchedules.CreateThreeStopSchedule(time)).Service
            });
        }
        
        [Fact]
        public void GatherSingleServiceBefore()
        {
            var schedule = CreateMockSchedule();
            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneBefore);
            var services = gatherer.Gather(2, TestDate);

            Assert.Single(services);
            var stop = services[0].Stop as ScheduleOrigin;
            Assert.Equal(TestStations.Surbiton, stop.Station);
            Assert.Equal(TestSchedules.Ten, stop.Departure);
        }
        
        [Fact]
        public void GatherServicesBeforeAndAfter()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneBeforeTwoAfter);
            var services = gatherer.Gather(1, TestDate);

            Assert.Equal(3, services.Length);
        }
        
        [Fact]
        public void GatheredServicesInAscendingTime()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneBeforeTwoAfter);
            var services = gatherer.Gather(1, TestDate);

            var first = services[0].Stop as ScheduleOrigin;
            var second = services[1].Stop as ScheduleOrigin;
            var third = services[2].Stop as ScheduleOrigin;

            Assert.True(first.Departure.Value < second.Departure.Value);
            Assert.True(second.Departure.Value < third.Departure.Value);
        }
        
        [Fact]
        public void GatherAsManyAsCanFindGoingBackward()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.Create(2, 0));
            var services = gatherer.Gather(1, TestDate);

            Assert.Equal(1, services.Length);    //TODO Loop to next day
        }
        
        [Fact]
        public void GatherAsManyAsCanFindGoingForward()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.Create(0, 2));
            var services = gatherer.Gather(3, TestDate);

            Assert.Equal(1, services.Length);    //TODO Loop to next day
        }
        
        [Fact]
        public void WillGatherMoreServicesIfHaveMultipleAtTheSameTime()
        {
            var responses = new []
            {
                (idx: 0, services: CreateTimeEntry(TestSchedules.Ten.AddMinutes(-60))), 
                (idx: 1, services: CreateTimeEntry(TestSchedules.Ten)), 
                (idx: 2, services: (TestSchedules.TenFifteen, new[]
                {
                    TestSchedules.CreateScheduleWithService(locations: TestSchedules.CreateThreeStopSchedule(TestSchedules.TenFifteen)).Service,
                    TestSchedules.CreateScheduleWithService(timetableId: "Y999999", locations: TestSchedules.CreateThreeStopSchedule(TestSchedules.TenFifteen)).Service
                })),
                (idx: 3, services: CreateTimeEntry(TestSchedules.TenThirty))
            };
            
            var schedule = CreateMockSchedule(responses);
            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneBeforeTwoAfter);
            var services = gatherer.Gather(1, TestDate);

            Assert.Equal(4, services.Length);    //TODO Loop to next day
        }
        
        [Fact]
        public void OnlyServicesThatSatisfyTheFilterAreReturned()
        {
            var schedule = CreateMockSchedule();
            var filterDelegate = Substitute.For<GatherFilterFactory.GatherFilter>();
            // First one found satisfies, rest do not
            filterDelegate(Arg.Any<ResolvedServiceStop>()).Returns(true, false);
            var config = new GatherConfiguration(0, 2, filterDelegate);
            var gatherer = new ScheduleGatherer(schedule, config);
            
            var services = gatherer.Gather(1, TestDate);

            Assert.Single(services);
        }
    }
}