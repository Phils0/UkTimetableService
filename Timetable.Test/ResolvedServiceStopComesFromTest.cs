using System;
using System.Collections.Generic;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceStopComesFromTest
    {
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
        
    }
}