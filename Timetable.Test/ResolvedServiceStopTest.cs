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

        
        public static IEnumerable<object[]> Stations
        {
            get
            {
                yield return new object[] {TestStations.Surbiton, false};   
                yield return new object[] {TestStations.ClaphamJunction, false};    // Destination is our stop.  Effectively assumes we do not have the same location twice in the stops list 
                yield return new object[] {TestStations.Waterloo, true};
                yield return new object[] {TestStations.Woking, false};
            }
        }
        
        [Theory]
        [MemberData(nameof(Stations))]
        public void GoesTo(Station station, bool expected)
        {
            var service =  TestSchedules.CreateService();
            var clapham = service.Details.Locations[1];
            var stop = new ResolvedServiceStop(service, clapham);
            
            Assert.Equal(expected, stop.GoesTo(station));
        }
    }
}