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
        public void ToStringReturnsServiceAndStop()
        {
            var service =  TestSchedules.CreateService();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            Assert.Equal("X12345 12/08/2019 10:00 SUR-SURBITN", stop.ToString());
        }
        
        public static IEnumerable<object[]> ToStations
        {
            get
            {
                yield return new object[] {TestStations.Surbiton, false};   
                yield return new object[] {TestStations.ClaphamJunction, false};    // Destination is our stop.  Effectively assumes we do not have the same location twice in the stops list 
                yield return new object[] {TestStations.Vauxhall, false};
                yield return new object[] {TestStations.Waterloo, true};
                yield return new object[] {TestStations.Woking, false};
            }
        }
        
        [Theory]
        [MemberData(nameof(ToStations))]
        public void GoesTo(Station station, bool isTo)
        {
            var service =  TestSchedules.CreateService();
            var clapham = service.Details.Locations[1];
            var stop = new ResolvedServiceStop(service, clapham);

            if (isTo)
            {
                Assert.True(stop.GoesTo(station));
                Assert.NotNull(stop.FoundToStop);
            }
            else
            {
                Assert.False(stop.GoesTo(station));
                Assert.Null(stop.FoundToStop);
            }
        }

        [Fact]
        public void GoesToAtPickUpOnlyIsFalse()
        {
            var service =  TestSchedules.CreateService();
            var surbiton = service.Details.Locations[0];
            var clapham = service.Details.Locations[1] as ScheduleStop;
            clapham.Arrival = Time.NotValid;
            clapham.Activities = new HashSet<string>(new [] {Activity.PickUpOnlyStop});
            clapham.UpdateAdvertisedStop();
            
            var stop = new ResolvedServiceStop(service, surbiton);
            
            Assert.False(stop.GoesTo(clapham.Station));
            Assert.Null(stop.FoundToStop);
        }
        
        public static IEnumerable<object[]> FromStations
        {
            get
            {
                yield return new object[] {TestStations.Surbiton, true};   
                yield return new object[] {TestStations.ClaphamJunction, false};    // Destination is our stop.  Effectively assumes we do not have the same location twice in the stops list 
                yield return new object[] {TestStations.Vauxhall, false};
                yield return new object[] {TestStations.Waterloo, false};
                yield return new object[] {TestStations.Woking, false};
            }
        }
        
        [Theory]
        [MemberData(nameof(FromStations))]
        public void ComesFrom(Station station, bool isFrom)
        {
            var service =  TestSchedules.CreateService();
            var clapham = service.Details.Locations[1];
            var stop = new ResolvedServiceStop(service, clapham);

            if (isFrom)
            {
                Assert.True(stop.ComesFrom(station));
                Assert.NotNull(stop.FoundFromStop);
            }
            else
            {
                Assert.False(stop.ComesFrom(station));
                Assert.Null(stop.FoundFromStop);
            }
        }
        
        [Fact]
        public void ComesFromAtSetDownOnlyIsFalse()
        {
            var service =  TestSchedules.CreateService();
            var waterloo = service.Details.Locations[3];
            var clapham = service.Details.Locations[1] as ScheduleStop;
            clapham.Departure = Time.NotValid;
            clapham.Activities = new HashSet<string>(new [] {Activity.SetDownOnlyStop});
            clapham.UpdateAdvertisedStop();
            
            var stop = new ResolvedServiceStop(service, waterloo);
            
            Assert.False(stop.GoesTo(clapham.Station));
            Assert.Null(stop.FoundFromStop);
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