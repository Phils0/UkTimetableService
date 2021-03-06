using System;
using System.Collections.Generic;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class PublicScheduleTest
    {
        [Fact]
        public void AddServiceOnlyWhenPublic()
        {
            var publicStop = CreateScheduleStop(TestSchedules.Ten);
            var workingStop = CreateScheduleStop(TestSchedules.TenThirty);
            workingStop.Arrival = Time.NotValid;

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals,  Time.EarlierLaterComparer);
            schedule.AddService(CreateServiceTime(publicStop));
            schedule.AddService(CreateServiceTime(workingStop));

            Assert.NotEmpty(schedule.GetServices(TestSchedules.Ten));
            Assert.Empty(schedule.GetServices(TestSchedules.TenThirty));
        }

        private ScheduleStop CreateScheduleStop(Time time, string timetableId = "X12345", ICalendar calendar = null)
        {
            calendar = calendar ?? TestSchedules.EverydayAugust2019;
            var stop = TestScheduleLocations.CreateStop(TestStations.Surbiton, time);
            TestSchedules.CreateScheduleWithService(timetableId: timetableId, calendar: calendar, stops: new ScheduleLocation[] {stop});
            return stop;
        }

        private IServiceTime CreateServiceTime(ScheduleStop stop)
        {
            return new ArrivalServiceTime(stop);
        }
        
        [Fact]
        public void SupportsMultipleServicesAtTheSameTime()
        {
            var stop1 = CreateScheduleStop(TestSchedules.Ten);
            var stop2 = CreateScheduleStop(TestSchedules.Ten);

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            schedule.AddService(CreateServiceTime(stop1));
            schedule.AddService(CreateServiceTime(stop2));

            var services = schedule.GetServices(TestSchedules.Ten);
            Assert.Equal(2, services.Length);
        }

        [Fact]
        public void ReturnsTimeAndServicesAtIndex()
        {
            var stop1 = CreateScheduleStop(TestSchedules.Ten);
            var stop2 = CreateScheduleStop(TestSchedules.TenThirty);

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            schedule.AddService(CreateServiceTime(stop1));
            schedule.AddService(CreateServiceTime(stop2));

            var pair = schedule.ValuesAt(1);
            Assert.Equal(TestSchedules.TenThirty, pair.time);
            Assert.Equal(stop2.Service, pair.services.Single());
        }
        
        [Fact]
        public void CountOfUniqueTimes()
        {
            var stop1 = CreateScheduleStop(TestSchedules.Ten);
            var stop2 = CreateScheduleStop(TestSchedules.TenThirty);
            var stop3 = CreateScheduleStop(TestSchedules.Ten);

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            schedule.AddService(CreateServiceTime(stop1));
            schedule.AddService(CreateServiceTime(stop2));
            schedule.AddService(CreateServiceTime(stop3));

            Assert.Equal(2, schedule.Count);
        }
        
        public static IEnumerable<object[]> Times
        {
            get
            {
                yield return new object[] {new TimeSpan(9, 59, 0), 0};
                yield return new object[] {new TimeSpan(10, 00, 0), 0};
                yield return new object[] {new TimeSpan(10, 01, 0), 1};
                yield return new object[] {new TimeSpan(10, 30, 0), 1};
            }
        }

        private static DateTime Aug4 = new DateTime(2019, 8, 4);
        private static DateTime Aug5 = Aug4.AddDays(1);
       
        [Theory]
        [MemberData(nameof(Times))]
        public void FindService(TimeSpan time, int expectedIdx)
        {
            var (schedule, services) = CreateSchedule();

            var searchAt = Aug5.Add(time);
            var found = schedule.FindServices(searchAt, GathererConfig.OneService);

            var expected =  services[expectedIdx].Schedule;
            Assert.Equal(expected, found[0].Service.Details);
        }

        private (PublicSchedule schedule, ScheduleStop[] stops) CreateSchedule(ICalendar calendar = null)
        {
            var services = new[]
            {
                CreateScheduleStop(TestSchedules.Ten, calendar: calendar),
                CreateScheduleStop(TestSchedules.TenThirty, calendar: calendar),
                CreateScheduleStop(new Time(new TimeSpan(11, 0, 0)), calendar: calendar)
            };

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            foreach (var service in services)
            {
                schedule.AddService(CreateServiceTime(service));
            }

            return (schedule, services);
        }

        // TODO This is an initial version, fix to return nearest time
        [Fact]
        public void AlwaysReturnOneResult()
        {
            var services = new[]
            {
                CreateScheduleStop(TestSchedules.Ten),
                CreateScheduleStop(TestSchedules.TenThirty),
            };

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            foreach (var service in services)
            {
                schedule.AddService(CreateServiceTime(service));
            }

            var searchAt = Aug5.AddHours(10).AddMinutes(1);
            var found = schedule.FindServices(searchAt, GathererConfig.Create(0, 0));
            
            Assert.Equal(services[1].Schedule, found[0].Service.Details);
        }
        
        [Fact]
        public void CanReturnOnlyBeforeResults()
        {
            var services = new[]
            {
                CreateScheduleStop(TestSchedules.Ten),
                CreateScheduleStop(TestSchedules.TenThirty),
            };

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            foreach (var service in services)
            {
                schedule.AddService(CreateServiceTime(service));
            }

            var searchAt = Aug5.AddHours(10).AddMinutes(1);
            var found = schedule.FindServices(searchAt, GathererConfig.OneBefore);
            
            Assert.Equal(services[0].Schedule, found[0].Service.Details);
        }
        
        [Fact]
        public void ReturnsOnTimeResultWhenAskOnlyForBeforeResults()
        {
            var services = new[]
            {
                CreateScheduleStop(TestSchedules.Ten),
                CreateScheduleStop(TestSchedules.TenThirty),
            };

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            foreach (var service in services)
            {
                schedule.AddService(CreateServiceTime(service));
            }

            var searchAt = Aug5.AddHours(10);
            var found = schedule.FindServices(searchAt, GathererConfig.OneBefore);
            
            Assert.Single(found);
            Assert.Equal(services[0].Schedule, found[0].Service.Details);
        }
        
        [Fact]
        public void ReturnNearestWhenHaveVersionGoesOverMidnight()
        {
            var one = new Time(new TimeSpan(1, 0 , 0));
            var twelvethirty = new Time(new TimeSpan(0, 30, 0));
            twelvethirty = twelvethirty.Add(Time.OneDay);
            
            var services = new[]
            {
                CreateScheduleStop(one),
                CreateScheduleStop(twelvethirty)
            };

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            foreach (var service in services)
            {
                schedule.AddService(CreateServiceTime(service));
            }

            var searchAt = Aug5.AddMinutes(1);
            var found = schedule.FindServices(searchAt, GathererConfig.OneService);
            
            Assert.Equal(services[1].Schedule, found[0].Service.Details);
        }
        
        [Fact]
        public void ServiceDateIsDayBeforeWhenTimeIsPlusOneDay()
        {
            var one = new Time(new TimeSpan(1, 0 , 0));
            var twelvethirty = new Time(new TimeSpan(0, 30, 0));
            twelvethirty = twelvethirty.Add(Time.OneDay);
            
            var services = new[]
            {
                CreateScheduleStop(one),
                CreateScheduleStop(twelvethirty)
            };

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            foreach (var service in services)
            {
                schedule.AddService(CreateServiceTime(service));
            }

            var searchAt = Aug5.AddMinutes(1);
            var found = schedule.FindServices(searchAt, GathererConfig.OneService);
            
            Assert.Equal(Aug4, found[0].Service.On);
        }
        
        [Fact]
        public void WhenHaveTwoAtSameTimeJustOnePlusOneDay()
        {
            var twelvethirty = new Time(new TimeSpan(0, 30, 0));
            var twelvethirtyPlusOne = twelvethirty.Add(Time.OneDay);
            
            var services = new[]
            {
                CreateScheduleStop(twelvethirtyPlusOne, "NextDay"),
                CreateScheduleStop(twelvethirty, "Today")
            };

            var schedule = new PublicSchedule(TestStations.Surbiton, TimesToUse.Arrivals, Time.EarlierLaterComparer);
            foreach (var service in services)
            {
                schedule.AddService(CreateServiceTime(service));
            }

            var searchAt = Aug5.AddMinutes(1);
            var found = schedule.FindServices(searchAt, GathererConfig.OneService);
            
            Assert.Equal(2, found.Length);
            Assert.Collection(found, 
                s => { Assert.Equal(Aug4, s.On);},
                s => { Assert.Equal(Aug5, s.On); });
        }
        
        [Fact]
        public void GatherAll()
        {
            var (schedule, expected) = CreateSchedule();
            
            var found = schedule.AllServices(Aug5, GatherFilterFactory.NoFilter, Time.Midnight);
            Assert.Equal(expected.Length, found.Length);
        }
        
        [Fact]
        public void GatherAllReturnsEmptyWhenNoSchedules()
        {
            var (schedule, expected) = CreateSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.None));
            
            var services = schedule.AllServices(Aug5, GatherFilterFactory.NoFilter, Time.Midnight);
            Assert.Empty(services);
        }
    }
}