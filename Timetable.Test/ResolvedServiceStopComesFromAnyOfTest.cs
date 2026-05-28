using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceStopComesFromAnyOfTest
    {
        // Default service: Surbiton (10:00) -> Clapham Junction (10:15) -> Vauxhall (pass) -> Waterloo (10:30)

        [Fact]
        public void ComesFromAnyOfReturnsTrueWhenSomeMatch()
        {
            var stop = CreateWaterlooArrivalStop();
            var set = new HashSet<Station> { TestStations.Woking, TestStations.Surbiton };

            Assert.True(stop.ComesFromAnyOf(set));
            Assert.NotNull(stop.FoundFromStop);
            Assert.Equal(TestStations.Surbiton, stop.FoundFromStop.Stop.Station);
        }

        [Fact]
        public void ComesFromAnyOfReturnsFalseWhenNoneMatch()
        {
            var stop = CreateWaterlooArrivalStop();
            var set = new HashSet<Station> { TestStations.Woking, TestStations.Weybridge };

            Assert.False(stop.ComesFromAnyOf(set));
            Assert.Null(stop.FoundFromStop);
        }

        [Fact]
        public void ComesFromAnyOfPicksEarliestDepartureWhenMultipleMatch()
        {
            // Surbiton is the origin; Clapham Junction is intermediate.
            // Forward scan should encounter Surbiton first.
            var stop = CreateWaterlooArrivalStop();
            var set = new HashSet<Station> { TestStations.Surbiton, TestStations.ClaphamJunction };

            Assert.True(stop.ComesFromAnyOf(set));
            Assert.Equal(TestStations.Surbiton, stop.FoundFromStop.Stop.Station);
        }

        [Fact]
        public void ComesFromAnyOfReturnsFalseForEmptySet()
        {
            var stop = CreateWaterlooArrivalStop();
            var set = new HashSet<Station>();

            Assert.False(stop.ComesFromAnyOf(set));
            Assert.Null(stop.FoundFromStop);
        }

        private static ResolvedServiceStop CreateWaterlooArrivalStop()
        {
            var service = TestSchedules.CreateService();
            // Index 3 is Waterloo (terminus)
            return new ResolvedServiceStop(service, service.Details.Locations[3]);
        }
    }
}