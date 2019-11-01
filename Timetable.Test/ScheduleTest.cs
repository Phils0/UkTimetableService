using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy.Generators.Emitters;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

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
        public void HasRetailServiceIdChecksUsingTheShortRetailServiceId(string retailsServiceId, string testId, bool expected)
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
            Assert.Equal(expected, schedule.ShortRetailServiceId);
        }
        
        [Fact]
        public void GetsScheduleRunningOnDate()
        {
            var schedule = TestSchedules.CreateSchedule(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
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
            Assert.Equal(expected, schedule.OperatedBy(toc));
        }

        public static IEnumerable<object[]> Stops
        {
            get
            {
                var stops = TestSchedules.DefaultLocations;
                var origin = stops[0] as ScheduleOrigin;
                yield return new object[] {origin.Station, origin.Departure, TimesToUse.Departures};
                var intermediate = stops[1] as ScheduleStop;
                yield return new object[] {intermediate.Station, intermediate.Arrival, TimesToUse.Arrivals};
                yield return new object[] {intermediate.Station, intermediate.Departure, TimesToUse.Departures};
                var destination = stops[3] as ScheduleDestination;
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

        private StopSpecification CreateFindSpec(Station station, Time time, TimesToUse arrivalOrDeparture = TimesToUse.Departures)
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
    }
}