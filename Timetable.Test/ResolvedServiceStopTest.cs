using System;
using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceStopTest
    {
        private static readonly DateTime TestDate = new DateTime(2019, 8, 12);

        [Fact]
        public void OnReturnsResolvedServiceOn()
        {
            var service =  TestSchedules.CreateService();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            Assert.Equal(TestDate, stop.On);
        }
        
        [Fact]
        public void OperatorReturnsResolvedServiceOperator()
        {
            var service =  TestSchedules.CreateService();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            Assert.Equal("VT", stop.Operator.Code);
        }
        
        [Fact]
        public void ToStringReturnsServiceAndStop()
        {
            var service =  TestSchedules.CreateService();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            Assert.Equal("X12345 2019-08-12 10:00 SUR-SURBITN", stop.ToString());
        }
        
        public static IEnumerable<object[]> StartTimes
        {
            get
            {
                yield return new object[] {new Time(new TimeSpan(23, 0, 0)), false};  
                yield return new object[] {new Time(new TimeSpan(23, 55, 0)), true};   
                yield return new object[] {new Time(new TimeSpan(23, 40, 0)), false};     // Some stops the next day but not our one  
            }
        }
        
        [Theory]
        [MemberData(nameof(StartTimes))]
        public void StopIsNextDay(Time startTime, bool expected)
        {
            var stops = TestSchedules.CreateThreeStopSchedule(startTime);
            var service =  TestSchedules.CreateService(stops: stops);
            var clapham = service.Details.Locations[1];
            var stop = new ResolvedServiceStop(service, clapham);

            Assert.Equal(expected, stop.IsNextDay(true));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void OnlyDepartureNextDay(bool useDeparture, bool expected)
        {
            var stops = TestSchedules.CreateThreeStopSchedule(new Time(new TimeSpan(23, 40, 0)));
            var service =  TestSchedules.CreateService(stops: stops);
            var clapham = service.Details.Locations[1] as ScheduleStop;
            clapham.Departure = new Time(new TimeSpan(0, 5, 0)).AddDay();
            
            var stop = new ResolvedServiceStop(service, clapham);

            Assert.Equal(expected, stop.IsNextDay(useDeparture));

        }
        
    }
}