using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceStopGoesToAnyOfTest
    {
        // Default service: Surbiton (10:00) -> Clapham Junction (10:15) -> Vauxhall (pass) -> Waterloo (10:30)

        [Fact]
        public void GoesToAnyOfReturnsTrueWhenSomeMatch()
        {
            var stop = CreateSurbitonStop();
            var set = new HashSet<Station> { TestStations.Woking, TestStations.Waterloo };

            Assert.True(stop.GoesToAnyOf(set));
            Assert.NotNull(stop.FoundToStop);
            Assert.Equal(TestStations.Waterloo, stop.FoundToStop.Stop.Station);
        }

        [Fact]
        public void GoesToAnyOfReturnsFalseWhenNoneMatch()
        {
            var stop = CreateSurbitonStop();
            var set = new HashSet<Station> { TestStations.Woking, TestStations.Weybridge };

            Assert.False(stop.GoesToAnyOf(set));
            Assert.Null(stop.FoundToStop);
        }

        [Fact]
        public void GoesToAnyOfPicksLatestArrivalWhenMultipleMatch()
        {
            // Clapham Junction is intermediate; Waterloo is the terminus.
            // Backward scan should encounter Waterloo first.
            var stop = CreateSurbitonStop();
            var set = new HashSet<Station> { TestStations.ClaphamJunction, TestStations.Waterloo };

            Assert.True(stop.GoesToAnyOf(set));
            Assert.Equal(TestStations.Waterloo, stop.FoundToStop.Stop.Station);
        }

        [Fact]
        public void GoesToAnyOfReturnsFalseForEmptySet()
        {
            var stop = CreateSurbitonStop();
            var set = new HashSet<Station>();

            Assert.False(stop.GoesToAnyOf(set));
            Assert.Null(stop.FoundToStop);
        }

        private static ResolvedServiceStop CreateSurbitonStop()
        {
            var service = TestSchedules.CreateService();
            return new ResolvedServiceStop(service, service.Details.Locations[0]); // Surbiton (origin)
        }
    }
}