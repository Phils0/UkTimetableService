using System;
using System.Collections.Generic;
using ReflectionMagic;
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

        [Theory]
        [InlineData(PublicStop.No, false)]
        [InlineData(PublicStop.Yes, true)]
        [InlineData(PublicStop.Request, true)]
        [InlineData(PublicStop.PickUpOnly, false)]
        [InlineData(PublicStop.SetDownOnly, true)]
        public void GoesToIsFalseIfNotPublicArrival(PublicStop advertised, bool expected)
        {
            var service =  TestSchedules.CreateService();
            var surbiton = service.Details.Locations[0];
            var clapham = service.Details.Locations[1] as ScheduleStop;
            
            var updateable = clapham.AsDynamic();
            updateable.AdvertisedStop = advertised;
            
            var stop = new ResolvedServiceStop(service, surbiton);
            
            Assert.Equal(expected, stop.GoesTo(clapham.Station));
            Assert.Equal(expected, stop.FoundToStop != null);
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
        
        [Theory]
        [InlineData(PublicStop.No, false)]
        [InlineData(PublicStop.Yes, true)]
        [InlineData(PublicStop.Request, true)]
        [InlineData(PublicStop.PickUpOnly, true)]
        [InlineData(PublicStop.SetDownOnly, false)]
        public void ComesFromIsFalseIfNotPublicDeparture(PublicStop advertised, bool expected)
        {
            var service =  TestSchedules.CreateService();
            var waterloo = service.Details.Locations[3];
            var clapham = service.Details.Locations[1] as ScheduleStop;
            var updateable = clapham.AsDynamic();
            updateable.AdvertisedStop = advertised;
            
            var stop = new ResolvedServiceStop(service, waterloo);
            
            Assert.Equal(expected, stop.ComesFrom(clapham.Station));
            Assert.Equal(expected, stop.FoundFromStop != null);
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