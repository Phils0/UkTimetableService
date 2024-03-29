using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy.Generators.Emitters;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class CifScheduleTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Fact]
        public void ParentSetToService()
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            var schedule = TestSchedules.CreateSchedule();
            schedule.AddToService(service);

            Assert.Same(service, schedule.Service);
        }

        [Fact]
        public void CanAddSchedulesWithDifferentStpIndicator()
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent,
                calendar: TestSchedules.EverydayAugust2019);
            var overlay = TestSchedules.CreateSchedule(indicator: StpIndicator.Override,
                calendar: TestSchedules.EverydayAugust2019);

            permanent.AddToService(service);
            overlay.AddToService(service);

            Assert.Same(service, permanent.Service);
            Assert.Same(service, overlay.Service);
        }

        // It happens we order by the calendar as need a unique order for the SortedList but could be anything that creates uniqueness
        [Fact]
        public void CanAddSchedulesWithSameStpIndicator()
        {
            var service = new CifService("X12345", Substitute.For<ILogger>());
            var permanent = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent,
                calendar: TestSchedules.EverydayAugust2019);
            var permanent2 = TestSchedules.CreateSchedule(indicator: StpIndicator.Permanent,
                calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));

            permanent.AddToService(service);
            permanent2.AddToService(service);

            Assert.Same(service, permanent.Service);
            Assert.Same(service, permanent2.Service);
        }

        [Fact]
        public void CannotAddScheduleWithDifferentTimetableUid()
        {
            var schedule = TestSchedules.CreateSchedule(timetableId: "A00002", indicator: StpIndicator.Permanent);

            var service = new CifService("A00001", Substitute.For<ILogger>());

            Assert.Throws<ArgumentException>(() => schedule.AddToService(service));
        }

        [Theory]
        [InlineData("VT123400", "VT1234", true)]
        [InlineData("VT123400", "VT123400", true)]
        [InlineData("VT123400", "VT123401", true)]
        [InlineData("VT123400", "VT999900", false)]
        [InlineData("VT123400", "", false)]
        [InlineData("VT123400", null, false)]
        [InlineData("", "VT999900", false)]
        [InlineData("", "", false)]
        [InlineData("", null, false)]
        [InlineData(null, "VT999900", false)]
        [InlineData(null, "", false)]
        [InlineData(null, null, false)]
        public void HasRetailServiceIdChecksUsingTheShortRetailServiceId(string retailsServiceId, string testId,
            bool expected)
        {
            var schedule = TestSchedules.CreateSchedule();
            schedule.RetailServiceId = retailsServiceId;
            Assert.Equal(expected, schedule.HasRetailServiceId(testId));
        }

        [Theory]
        [InlineData("VT123400", "VT1234")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void ShortRetailServiceId(string retailServiceId, string expected)
        {
            var schedule = TestSchedules.CreateSchedule();
            schedule.RetailServiceId = retailServiceId;
            Assert.Equal(expected, schedule.NrsRetailServiceId);
        }

        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule =
                TestSchedules.CreateSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));

            Assert.True(schedule.RunsOn(MondayAugust12));
            Assert.False(schedule.RunsOn(MondayAugust12.AddDays(1)));
        }

        [Theory]
        [InlineData(StpIndicator.Cancelled, true)]
        [InlineData(StpIndicator.New, false)]
        [InlineData(StpIndicator.Override, false)]
        [InlineData(StpIndicator.Permanent, false)]
        public void IsCancelled(StpIndicator indicator, bool expected)
        {
            var schedule = TestSchedules.CreateSchedule(indicator: indicator);
            Assert.Equal(expected, schedule.IsCancelled());
        }

        [Theory]
        [InlineData("VT", true)]
        [InlineData("GW", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void OperatedByToc(string toc, bool expected)
        {
            var schedule = TestSchedules.CreateSchedule();
            Assert.Equal(expected, schedule.IsOperatedBy(toc));
        }

        public static IEnumerable<object[]> Stops
        {
            get
            {
                var stops = TestSchedules.DefaultLocations;
                var origin = stops[0] as ScheduleStop;
                yield return new object[] {origin.Station, origin.Departure, TimesToUse.Departures};
                var intermediate = stops[1] as ScheduleStop;
                yield return new object[] {intermediate.Station, intermediate.Arrival, TimesToUse.Arrivals};
                yield return new object[] {intermediate.Station, intermediate.Departure, TimesToUse.Departures};
                var destination = stops[3] as ScheduleStop;
                yield return new object[] {destination.Station, destination.Arrival, TimesToUse.Arrivals};
            }
        }

        [Theory]
        [MemberData(nameof(Stops))]
        public void FindStop(Station station, Time time, TimesToUse arrivalOrDeparture)
        {
            var schedule = TestSchedules.CreateSchedule();
            var find = CreateFindSpec(station, time, arrivalOrDeparture);

            Assert.True(schedule.TryFindStop(find, out var stop));
            Assert.Equal(station, stop.Station);

            if (find.UseArrival)
            {
                IArrival arrival = (IArrival) stop;
                Assert.Equal(time, arrival.Time);
            }
            else
            {
                IDeparture departure = (IDeparture) stop;
                Assert.Equal(time, departure.Time);
            }
        }

        private StopSpecification CreateFindSpec(Station station, Time time,
            TimesToUse arrivalOrDeparture = TimesToUse.Departures)
        {
            return new StopSpecification(station, time, MondayAugust12, arrivalOrDeparture);
        }

        [Fact]
        public void DoNotFindStopWhenTimeDifferent()
        {
            var schedule = TestSchedules.CreateSchedule();
            var find = CreateFindSpec(TestStations.Surbiton, TestSchedules.TenThirty);

            Assert.False(schedule.TryFindStop(find, out var stop));
        }

        [Fact]
        public void DoNotFindStopWheenDoesNotStopAtStation()
        {
            var schedule = TestSchedules.CreateSchedule();
            var find = CreateFindSpec(TestStations.Woking, TestSchedules.TenThirty);

            Assert.False(schedule.TryFindStop(find, out var stop));
        }

        public static IEnumerable<object[]> Locations
        {
            get
            {
                yield return new object[] {TestLocations.Woking, 1, true};
                yield return new object[] {TestLocations.CLPHMJN, 1, false};
                yield return new object[] {TestLocations.CLPHMJN, 2, true};
                yield return new object[] {TestLocations.CLPHMJC, 1, true};
            }
        }

        [Theory]
        [MemberData(nameof(Locations))]
        public void GetStopFindsStop(Location location, int sequence, bool expectedException)
        {
            var schedule = TestSchedules.CreateSchedule();
            if (expectedException)
                Assert.Throws<ArgumentException>(() => schedule.GetStop(location, sequence));
            else
                Assert.NotNull(schedule.GetStop(location, sequence));
        }

        public static IEnumerable<object[]> Arrivals
        {
            get
            {
                var stops = TestSchedules.DefaultLocations;
                var origin = stops[0] as ScheduleStop;
                yield return new object[] {origin.Station, false};
                var intermediate = stops[1] as ScheduleStop;
                yield return new object[] {intermediate.Station, true};
                var pass = stops[2] as SchedulePass;
                yield return new object[] {pass.Station, false};
                var destination = stops[3] as ScheduleStop;
                yield return new object[] {destination.Station, true};
            }
        }

        [Theory]
        [MemberData(nameof(Arrivals))]
        public void InArrivals(Station location, bool expectedContains)
        {
            var schedule = TestSchedules.CreateSchedule();

            if (expectedContains)
                Assert.Contains<Station>(location, schedule.Arrivals.Select(a => a.Station));
            else
                Assert.DoesNotContain<Station>(location, schedule.Arrivals.Select(a => a.Station));
        }

        public static IEnumerable<object[]> Departures
        {
            get
            {
                var stops = TestSchedules.DefaultLocations;
                var origin = stops[0] as ScheduleStop;
                yield return new object[] {origin.Station, true};
                var intermediate = stops[1] as ScheduleStop;
                yield return new object[] {intermediate.Station, true};
                var pass = stops[2] as SchedulePass;
                yield return new object[] {pass.Station, false};
                var destination = stops[3] as ScheduleStop;
                yield return new object[] {destination.Station, false};
            }
        }

        [Theory]
        [MemberData(nameof(Departures))]
        public void InDepartures(Station location, bool expectedContains)
        {
            var schedule = TestSchedules.CreateSchedule();

            if (expectedContains)
                Assert.Contains<Station>(location, schedule.Departures.Select(d => d.Station));
            else
                Assert.DoesNotContain<Station>(location, schedule.Departures.Select(d => d.Station));
        }

        public static Time Ten => TestSchedules.Ten;

        public static TheoryData<CifSchedule, bool> Schedules => new TheoryData<CifSchedule, bool>()
        {
            {
                TestSchedules.CreateSchedule(stops: TestSchedules.DefaultLocations), 
                true
            },
            {
                TestSchedules.CreateSchedule(stops: new ScheduleLocation[]
                {
                    TestScheduleLocations.CreatePass(TestStations.Vauxhall, Ten.AddMinutes(20)),
                    TestScheduleLocations.CreateDestination(TestStations.Waterloo, Ten.AddMinutes(30))
                }),
                false
            },
            {
                TestSchedules.CreateSchedule(stops: new ScheduleLocation[]
                {
                    TestScheduleLocations.CreateOrigin(TestStations.Surbiton, Ten),
                    TestScheduleLocations.CreatePass(TestStations.Vauxhall, Ten.AddMinutes(20)),
                }),
                false
            },
            {
                TestSchedules.CreateSchedule(stops: Array.Empty<ScheduleLocation>()),
                false
            },
            {
                TestSchedules.CreateSchedule(stops: Array.Empty<ScheduleLocation>(), indicator: StpIndicator.Cancelled),
                true
            },
            {
                TestSchedules.CreateSchedule(stops: Array.Empty<ScheduleLocation>(), sleeperClass: AccomodationClass.Both),
                true
            }             
        };

        [Theory]
        [MemberData(nameof(Schedules))]
        public void IsPublicService(CifSchedule schedule, bool expected)
        {
            Assert.Equal(expected, schedule.IsPublicSchedule());
        }

        [Theory]
        [InlineData(AccomodationClass.First)]
        [InlineData(AccomodationClass.Standard)]
        [InlineData(AccomodationClass.Both)]
        public void IsSleeper(AccomodationClass sleeperClass)
        {
            var schedule = TestSchedules.CreateSchedule(sleeperClass: sleeperClass);
            Assert.True(schedule.IsSleeper());
        }   
        
        [Fact]
        public void IsNotSleeper()
        {
            var schedule = TestSchedules.CreateSchedule(sleeperClass: AccomodationClass.None);
            Assert.False(schedule.IsSleeper());
        }
    }
}