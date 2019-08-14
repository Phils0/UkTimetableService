using System;
using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleLocationIsStopAtTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        private static Time TenZeroOne => new Time(new TimeSpan(10, 1,0 ));

        private readonly Station Surbiton = TestStations.Surbiton;
        
        public static IEnumerable<object[]> StopTimes
        {
            get
            {
                yield return new object[] {TestSchedules.Ten, TestSchedules.Ten, TimesToUse.Arrivals, true};
                yield return new object[] {TestSchedules.Ten, TenZeroOne, TimesToUse.Arrivals, false};
                yield return new object[] {TestSchedules.Ten, TestSchedules.Ten, TimesToUse.Departures, false};
                yield return new object[] {TestSchedules.Ten, TenZeroOne, TimesToUse.Departures, true};
                yield return new object[] {TestSchedules.Ten, TestSchedules.TenThirty, TimesToUse.Arrivals, false};
                yield return new object[] {TestSchedules.Ten, TestSchedules.Ten.AddDay(), TimesToUse.Arrivals, true};
                yield return new object[] {TestSchedules.Ten.AddDay(), TestSchedules.Ten, TimesToUse.Arrivals, true};
                yield return new object[] {TestSchedules.Ten, TenZeroOne.AddDay(), TimesToUse.Departures, true};
                yield return new object[] {TestSchedules.Ten.AddDay(), TenZeroOne, TimesToUse.Departures, true};
            }
        }
        
        [Theory]
        [MemberData(nameof(StopTimes))]
        public void IsStopAtIfLocationMatchesAndPublicTimeMatches(Time stopArrival, Time findTime, TimesToUse arrivalOrDeparture, bool expected)
        {
            var stop = TestScheduleLocations.CreateStop(Surbiton, stopArrival);
            var spec = CreateFindSpec(findTime, arrivalOrDeparture);
            Assert.Equal(expected, stop.IsStopAt(spec));
        }
        private StopSpecification CreateFindSpec(Time findTime, TimesToUse arrivalOrDeparture)
        {
            return new StopSpecification(Surbiton, findTime, MondayAugust12, arrivalOrDeparture);
        }

        public static IEnumerable<object[]> DepartureOnlyTimes
        {
            get
            {
                yield return new object[] {TenZeroOne, TestSchedules.Ten, TimesToUse.Arrivals, false};
                yield return new object[] {TenZeroOne, TenZeroOne, TimesToUse.Arrivals, false};
                yield return new object[] {TenZeroOne, TestSchedules.Ten, TimesToUse.Departures, false};
                yield return new object[] {TenZeroOne, TenZeroOne, TimesToUse.Departures, true};
                yield return new object[] {TenZeroOne, TestSchedules.TenThirty, TimesToUse.Arrivals, false};
                yield return new object[] {TenZeroOne, TenZeroOne.AddDay(), TimesToUse.Departures, true};
                yield return new object[] {TenZeroOne, TenZeroOne, TimesToUse.Departures, true};
            }
        }
        
        [Theory]
        [MemberData(nameof(DepartureOnlyTimes))]
        public void IsStopAtWorksForPickupOnlyPickupOnly(Time stopDeparture, Time findTime, TimesToUse arrivalOrDeparture, bool expected)
        {
            var stop = TestScheduleLocations.CreatePickupOnlyStop(Surbiton, stopDeparture);
            stop.WorkingArrival = stopDeparture.AddMinutes(-1);
            
            var spec = CreateFindSpec(findTime, arrivalOrDeparture);
            Assert.Equal(expected, stop.IsStopAt(spec));
        }
        
        public static IEnumerable<object[]> ArrivalOnlyTimes
        {
            get
            {
                yield return new object[] {TestSchedules.Ten, TestSchedules.Ten, TimesToUse.Arrivals, true};
                yield return new object[] {TestSchedules.Ten, TenZeroOne, TimesToUse.Arrivals, false};
                yield return new object[] {TestSchedules.Ten, TestSchedules.Ten, TimesToUse.Departures, false};
                yield return new object[] {TestSchedules.Ten, TenZeroOne, TimesToUse.Departures, false};
                yield return new object[] {TestSchedules.Ten, TestSchedules.TenThirty, TimesToUse.Arrivals, false};
                yield return new object[] {TestSchedules.Ten, TestSchedules.Ten.AddDay(), TimesToUse.Arrivals, true};
                yield return new object[] {TestSchedules.Ten.AddDay(), TestSchedules.Ten, TimesToUse.Arrivals, true};
            }
        }
        
        [Theory]
        [MemberData(nameof(ArrivalOnlyTimes))]
        public void IsStopAtWorksForSetdownOnly(Time stopArrival, Time findTime, TimesToUse arrivalOrDeparture, bool expected)
        {
            var stop = TestScheduleLocations.CreateSetdownOnlyStop(Surbiton, stopArrival);
            stop.WorkingDeparture = stopArrival.AddMinutes(1);
            var spec = CreateFindSpec(findTime, arrivalOrDeparture);
            Assert.Equal(expected, stop.IsStopAt(spec));
        }

        [Theory]
        [MemberData(nameof(ArrivalOnlyTimes))]
        public void IsStopAtWorksForDestination(Time stopArrival, Time findTime, TimesToUse arrivalOrDeparture, bool expected)
        {
            var stop = TestScheduleLocations.CreateDestination(Surbiton, stopArrival);
            var spec = CreateFindSpec(findTime, arrivalOrDeparture);
            Assert.Equal(expected, stop.IsStopAt(spec));
        }

        [Theory]
        [MemberData(nameof(DepartureOnlyTimes))]
        public void IsStopAtWorksForOrigin(Time stopDeparture, Time findTime, TimesToUse arrivalOrDeparture, bool expected)
        {
            var stop = TestScheduleLocations.CreateOrigin(Surbiton, stopDeparture);
            var spec = CreateFindSpec(findTime, arrivalOrDeparture);
            Assert.Equal(expected, stop.IsStopAt(spec));
        }

        [Theory]
        [InlineData(TimesToUse.Arrivals)]
        [InlineData(TimesToUse.Departures)]
        public void IsStopAtAlwaysFalseForPassingPoint(TimesToUse arrivalOrDeparture)
        {
            var stop = TestScheduleLocations.CreatePass(Surbiton, TestSchedules.Ten);
            var spec = CreateFindSpec(TestSchedules.Ten, arrivalOrDeparture);
            Assert.False(stop.IsStopAt(spec));
        }
        
    }
}