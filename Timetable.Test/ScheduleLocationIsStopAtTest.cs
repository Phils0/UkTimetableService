using System;
using System.Collections.Generic;
using System.Linq;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleLocationIsStopAtTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        private static Time TenZeroOne => new Time(new TimeSpan(10, 1,0 ));

        private static readonly Station Surbiton = TestStations.Surbiton;
        private static readonly Station ClaphamJunction = TestStations.ClaphamJunction;      
        
        public static IEnumerable<object[]> Locations
        {
            get
            {
                yield return new object[] {Surbiton.Locations.Single(), false};
                yield return new object[] {ClaphamJunction.Locations.First(), true};
                yield return new object[] {ClaphamJunction.Locations.Last(), true};
            }
        }
        
        [Theory]
        [MemberData(nameof(Locations))]
        public void IsStopAtIfLocationMatches(Location location, bool expected)
        {
            var stop = TestScheduleLocations.CreateStop(location, TestSchedules.Ten);
            var spec = CreateFindSpec(TestSchedules.Ten, TimesToUse.Arrivals, ClaphamJunction);
            Assert.Equal(expected, stop.IsStopAt(spec));
        }

        public static TheoryData<PublicStop, bool, bool> AdvertisedStop =>
            new TheoryData<PublicStop, bool, bool>()
            {
                {PublicStop.No, true, false},
                {PublicStop.No, false, false},
                {PublicStop.Yes, true, true},
                {PublicStop.Yes, false, true},          
                {PublicStop.PickUpOnly, true, false},
                {PublicStop.PickUpOnly, false, true},
                {PublicStop.SetDownOnly, true, true},
                {PublicStop.SetDownOnly, false, false},                  
                {PublicStop.Request, true, true},
                {PublicStop.Request, false, true}
            };
        
        [Theory]
        [MemberData(nameof(AdvertisedStop))]
        public void OnlyIncludeStopsWhereAdvertisedStopIsRight(PublicStop advertised, bool useArrivals, bool expected)
        {
            var stop = TestScheduleLocations.CreateStop(ClaphamJunction, TestSchedules.Ten);
            var updateable = stop.AsDynamic();
            updateable.AdvertisedStop = advertised;

            var arrivalsDepartures = useArrivals ? TimesToUse.Arrivals : TimesToUse.Departures;
            var findAt = useArrivals ? TestSchedules.Ten : TestSchedules.Ten.AddMinutes(1);
            var spec = CreateFindSpec(findAt, arrivalsDepartures, ClaphamJunction);
            Assert.Equal(expected, stop.IsStopAt(spec));
        }
        
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
        
        private StopSpecification CreateFindSpec(Time findTime, TimesToUse arrivalOrDeparture, Station atLocation = null)
        {
            atLocation = atLocation ?? Surbiton;
            return new StopSpecification(atLocation, findTime, MondayAugust12, arrivalOrDeparture);
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
        
        public static IEnumerable<object[]> IsStopLocations
        {
            get
            {
                yield return new object[] {Surbiton.Locations.Single(), 1, false};
                yield return new object[] {ClaphamJunction.Locations.First(), 1, true};
                yield return new object[] {ClaphamJunction.Locations.First(), 2, false};
                yield return new object[] {ClaphamJunction.Locations.Last(), 1, false};
            }
        }
        
        [Theory]
        [MemberData(nameof(IsStopLocations))]
        public void IsStopIfLocationMatches(Location location, int sequence, bool expected)
        {
            var stop = TestScheduleLocations.CreateStop(ClaphamJunction.Locations.First(), TestSchedules.Ten);
            Assert.Equal(expected, stop.IsStop(location, sequence));
        }
    }
}