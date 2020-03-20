using System;
using System.Collections.Generic;
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
            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneService, TimesToUse.Departures);
            var services = gatherer.Gather(2, TestDate);

            Assert.Single(services);
            var stop = services[0].Stop.Stop as ScheduleStop;
            Assert.Equal(TestStations.Surbiton, stop.Station);
            Assert.Equal(TestSchedules.TenFifteen, stop.Departure);
        }

        private IGathererScheduleData CreateMockSchedule((int idx, (Time, Service[]) services)[] responses = null, ICalendar calendar = null)
        {
            responses = responses ?? (new []
            {
                (idx: 0, services: CreateTimeEntry(TestSchedules.Ten.AddMinutes(-60), calendar)), 
                (idx: 1, services: CreateTimeEntry(TestSchedules.Ten, calendar)), 
                (idx: 2, services: CreateTimeEntry(TestSchedules.TenFifteen, calendar)),
                (idx: 3, services: CreateTimeEntry(TestSchedules.TenThirty, calendar))
            });
            
            var schedule = Substitute.For<IGathererScheduleData>();
            schedule.Location.Returns(TestStations.Surbiton);
            schedule.Count.Returns(responses.Length);
            foreach (var tuple in responses)
            {
                schedule.ValuesAt(tuple.idx).Returns(tuple.services);
            }

            return schedule;
        }

        private (Time, Service[]) CreateTimeEntry(Time time, ICalendar calendar = null)
        {
            calendar = calendar ?? TestSchedules.EverydayAugust2019;
            return (time, new[]
            {
                TestSchedules.CreateScheduleWithService(
                    calendar: calendar,
                    stops: TestSchedules.CreateThreeStopSchedule(time)).Service
            });
        }

        [Fact]
        public void GatherMultipleServiceAfter()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.Create(0, 2), TimesToUse.Departures);
            var services = gatherer.Gather(1, TestDate);

            Assert.Equal(2, services.Length);
        }
        
        [Fact]
        public void GatherReturnsEmptyWhenNoneValid()
        {
            var schedule = CreateMockSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.None));

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.Create(0, 2), TimesToUse.Departures);
            var services = gatherer.Gather(1, TestDate);

            Assert.Empty(services);
        }
        
        [Fact]
        public void GatherSingleServiceBefore()
        {
            var schedule = CreateMockSchedule();
            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneBefore, TimesToUse.Departures);
            var services = gatherer.Gather(2, TestDate);

            Assert.Single(services);
            var stop = services[0].Stop.Stop as ScheduleStop;
            Assert.Equal(TestStations.Surbiton, stop.Station);
            Assert.Equal(TestSchedules.Ten, stop.Departure);
        }
        
        [Fact]
        public void GatherServicesBeforeAndAfter()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneBeforeTwoAfter, TimesToUse.Departures);
            var services = gatherer.Gather(1, TestDate);

            Assert.Equal(3, services.Length);
        }
        
        [Fact]
        public void GatheredServicesInAscendingTime()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneBeforeTwoAfter, TimesToUse.Departures);
            var services = gatherer.Gather(1, TestDate);

            var first = services[0].Stop.Stop as ScheduleStop;
            var second = services[1].Stop.Stop as ScheduleStop;
            var third = services[2].Stop.Stop as ScheduleStop;

            Assert.True(first.Departure.Value < second.Departure.Value);
            Assert.True(second.Departure.Value < third.Departure.Value);
        }
        
        [Fact]
        public void GatherAsManyAsCanFindGoingBackward()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.Create(2, 0), TimesToUse.Departures);
            var services = gatherer.Gather(1, TestDate);

            Assert.Equal(1, services.Length);    //TODO Loop to next day
        }
        
        [Fact]
        public void GatherAsManyAsCanFindGoingForward()
        {
            var schedule = CreateMockSchedule();

            var gatherer = new ScheduleGatherer(schedule, GathererConfig.Create(0, 2), TimesToUse.Departures);
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
                    TestSchedules.CreateScheduleWithService(stops: TestSchedules.CreateThreeStopSchedule(TestSchedules.TenFifteen)).Service,
                    TestSchedules.CreateScheduleWithService(timetableId: "Y999999", stops: TestSchedules.CreateThreeStopSchedule(TestSchedules.TenFifteen)).Service
                })),
                (idx: 3, services: CreateTimeEntry(TestSchedules.TenThirty))
            };
            
            var schedule = CreateMockSchedule(responses);
            var gatherer = new ScheduleGatherer(schedule, GathererConfig.OneBeforeTwoAfter, TimesToUse.Departures);
            var services = gatherer.Gather(1, TestDate);

            Assert.Equal(4, services.Length);    //TODO Loop to next day
        }
        
        [Fact]
        public void OnlyServicesThatSatisfyTheFilterAreReturned()
        {
            var schedule = CreateMockSchedule();
            // Simulate first one found satisfies, rest do not
            bool returnedOnce = false;
            IEnumerable<ResolvedServiceStop> Filter(IEnumerable<ResolvedServiceStop> s)
            {
                if (!returnedOnce)
                {
                    returnedOnce = true;
                    return s;
                }

                return Enumerable.Empty<ResolvedServiceStop>();
            }

            var config = new GatherConfiguration(0, 3, false, Filter);
            var gatherer = new ScheduleGatherer(schedule, config, TimesToUse.Departures);
            
            var services = gatherer.Gather(1, TestDate);

            Assert.Single(services);
        }
    }
}