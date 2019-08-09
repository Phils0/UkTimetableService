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
        [InlineData("VT123400", true)]
        [InlineData("VT999900", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsRetailServiceId(string retailServiceId, bool expected)
        {
            var schedule = TestSchedules.CreateSchedule();
            Assert.Equal(expected, schedule.HasRetailServiceId(retailServiceId));
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
                yield return new object[] {origin.Station, origin.Departure, false};
                var intermediate = stops[1] as ScheduleStop;
                yield return new object[] {intermediate.Station, intermediate.Arrival, true};
                yield return new object[] {intermediate.Station, intermediate.Departure, false};
                var destination = stops[2] as ScheduleDestination;
                yield return new object[] {destination.Station, destination.Arrival, true};
            }
        }

        [Theory]
        [MemberData(nameof(Stops))]
        public void FindStop(Station station, Time time, bool isArrival)
        {
            var schedule = TestSchedules.CreateSchedule();

            Assert.True(schedule.TryFindStop(station, time, out var stop));
            Assert.Equal(station, stop.Station);

            if (isArrival)
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
        
        [Fact]
        public void DoNotFindStopWhenTimeDifferent()
        {
            var schedule = TestSchedules.CreateSchedule();

            Assert.False(schedule.TryFindStop(TestStations.Surbiton, TestSchedules.TenThirty, out var stop));
        }
        
        [Fact]
        public void DoNotFindStopWheenDoesNotStopAtStation()
        {
            var schedule = TestSchedules.CreateSchedule();
            Assert.False(schedule.TryFindStop(TestStations.Woking, TestSchedules.TenThirty, out var stop));
        }
    }
}