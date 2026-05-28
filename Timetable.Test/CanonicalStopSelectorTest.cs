using System.Collections.Generic;
using System.Linq;
using Timetable.Test.Data;
using Xunit;
using static Timetable.Test.StationGroupSearchFixtures;

namespace Timetable.Test
{
    public class CanonicalStopSelectorTest
    {
        private static CanonicalStopSelector Selector(JourneyHeuristic heuristic) => new(heuristic);

        // EUS is a member that the test services never call at — handy for exercising the "priority is a valid
        // member but this service doesn't stop there" fallback path.
        private static StationGroup LondonOriginGroup(params string[] priorities) =>
            new("GB@LO", new[] { KingsCross, StPancras, Euston }, ToPriorities(priorities));

        private static StationGroup ManchesterDestGroup(params string[] priorities) =>
            new("GB@MA", new[] { ManchesterPiccadilly, ManchesterVictoria, ManchesterOxfordRoad }, ToPriorities(priorities));

        // Priority CRS codes resolve to stations the same way members do, so each priority compares equal to
        // its member by station identity.
        private static IReadOnlyList<Station>? ToPriorities(params string[] priorities) =>
            priorities.Length == 0 ? null : priorities.Select(TestStations.Create).ToArray();

        // ---------- ChooseDeparture ----------

        [Fact]
        public void Departure_SingleCandidatePassesThrough()
        {
            var service = EustonToManchesterAll();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]); // Euston

            var result = Selector(JourneyHeuristic.Longest).ChooseDeparture(new[] { stop }, LondonOriginGroup(), null);

            Assert.Same(stop, result);
        }

        [Fact]
        public void Departure_OriginGroupPicksEarliestDeparture_WithLongestHeuristic()
        {
            var (atKgx, atStp) = CreateLondonOriginCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseDeparture(new[] { atStp, atKgx }, LondonOriginGroup(), null);

            Assert.Equal(KingsCross, result!.Stop.Stop.Station);
        }

        [Fact]
        public void Departure_OriginGroupPicksLatestDeparture_WithShortestHeuristic()
        {
            var (atKgx, atStp) = CreateLondonOriginCandidates();

            var result = Selector(JourneyHeuristic.Shortest).ChooseDeparture(new[] { atKgx, atStp }, LondonOriginGroup(), null);

            Assert.Equal(StPancras, result!.Stop.Stop.Station);
        }

        [Fact]
        public void Departure_OriginPriority_WinsOverHeuristic()
        {
            // Longest alone would pick KGX (earliest); priorities=["STP"] forces St Pancras.
            var (atKgx, atStp) = CreateLondonOriginCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseDeparture(new[] { atKgx, atStp }, LondonOriginGroup("STP"), null);

            Assert.Equal(StPancras, result!.Stop.Stop.Station);
        }

        [Fact]
        public void Departure_OriginPriorityMiss_FallsBackToHeuristic()
        {
            // EUS is a valid group member but this service never departs there, so no candidate matches the
            // priority; fall back to Longest -> KGX.
            var (atKgx, atStp) = CreateLondonOriginCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseDeparture(new[] { atKgx, atStp }, LondonOriginGroup("EUS"), null);

            Assert.Equal(KingsCross, result!.Stop.Stop.Station);
        }

        [Fact]
        public void Departure_DestinationOverride_AppliesPriority()
        {
            // The filter's natural backward scan picks MAN (latest Manchester arrival).
            // priorities=["MCV"] must override that to Manchester Victoria.
            var service = EustonToManchesterAll();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            stop.GoesTo(ManchesterPiccadilly); // simulate filter

            var result = Selector(JourneyHeuristic.Longest).ChooseDeparture(new[] { stop }, null, ManchesterDestGroup("MCV"));

            Assert.Equal(ManchesterVictoria, result!.FoundToStop.Stop.Station);
        }

        [Fact]
        public void Departure_DestinationOverrideShortest_PicksEarliestArrival()
        {
            var service = EustonToManchesterAll();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            stop.GoesTo(ManchesterPiccadilly);

            var result = Selector(JourneyHeuristic.Shortest).ChooseDeparture(new[] { stop }, null, ManchesterDestGroup());

            Assert.Equal(ManchesterOxfordRoad, result!.FoundToStop.Stop.Station);
        }

        [Fact]
        public void Departure_DestinationOverrideIsNoOp_ForLongestWithoutPriorities()
        {
            var service = EustonToManchesterAll();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            stop.GoesTo(ManchesterPiccadilly);
            var preOverride = stop.FoundToStop;

            var result = Selector(JourneyHeuristic.Longest).ChooseDeparture(new[] { stop }, null, ManchesterDestGroup());

            Assert.Same(preOverride, result!.FoundToStop);
        }

        // ---------- ChooseArrival ----------

        [Fact]
        public void Arrival_DestinationGroupPicksLatestArrival_WithLongestHeuristic()
        {
            var (atMco, atMcv, atMan) = CreateManchesterDestinationCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseArrival(new[] { atMco, atMcv, atMan }, null, ManchesterDestGroup());

            Assert.Equal(ManchesterPiccadilly, result!.Stop.Stop.Station);
        }

        [Fact]
        public void Arrival_DestinationGroupPicksEarliestArrival_WithShortestHeuristic()
        {
            var (atMco, atMcv, atMan) = CreateManchesterDestinationCandidates();

            var result = Selector(JourneyHeuristic.Shortest).ChooseArrival(new[] { atMan, atMcv, atMco }, null, ManchesterDestGroup());

            Assert.Equal(ManchesterOxfordRoad, result!.Stop.Stop.Station);
        }

        [Fact]
        public void Arrival_DestinationPriority_WinsOverHeuristic()
        {
            // Longest alone would pick MAN; priorities=["MCV"] forces Manchester Victoria.
            var (atMco, atMcv, atMan) = CreateManchesterDestinationCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseArrival(new[] { atMco, atMcv, atMan }, null, ManchesterDestGroup("MCV"));

            Assert.Equal(ManchesterVictoria, result!.Stop.Stop.Station);
        }

        [Fact]
        public void Arrival_OriginOverride_AppliesPriority()
        {
            // Single candidate arriving at Manchester from London. Filter's natural pick (Longest = earliest
            // departure) would be KGX; priorities=["STP"] must override to St Pancras.
            var service = LondonAllToManchesterPiccadilly();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[2]); // Manchester
            stop.ComesFrom(KingsCross);

            var result = Selector(JourneyHeuristic.Longest).ChooseArrival(new[] { stop }, LondonOriginGroup("STP"), null);

            Assert.Equal(StPancras, result!.FoundFromStop.Stop.Station);
        }

        [Fact]
        public void Arrival_OriginOverride_ShortestPicksLatestDeparture()
        {
            var service = LondonAllToManchesterPiccadilly();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[2]);
            stop.ComesFrom(KingsCross);

            var result = Selector(JourneyHeuristic.Shortest).ChooseArrival(new[] { stop }, LondonOriginGroup(), null);

            Assert.Equal(StPancras, result!.FoundFromStop.Stop.Station);
        }

        // ---------- Both path and query are groups ----------

        [Fact]
        public void Departure_BothGroups_LongestSelectsEarliestOriginAndKeepsLatestDestination()
        {
            var (atKgx, atStp) = CreateLondonToManchesterOriginCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseDeparture(new[] { atStp, atKgx }, LondonOriginGroup(), ManchesterDestGroup());

            Assert.Equal(KingsCross, result!.Stop.Stop.Station);                    // earliest departure
            Assert.Equal(ManchesterPiccadilly, result.FoundToStop.Stop.Station);    // latest arrival (filter's pick kept)
        }

        [Fact]
        public void Arrival_BothGroups_LongestSelectsLatestDestinationAndKeepsEarliestOrigin()
        {
            var (atMco, atMcv, atMan) = CreateLondonToManchesterDestinationCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseArrival(new[] { atMco, atMcv, atMan }, LondonOriginGroup(), ManchesterDestGroup());

            Assert.Equal(ManchesterPiccadilly, result!.Stop.Stop.Station);  // latest arrival (path side)
            Assert.Equal(KingsCross, result.FoundFromStop.Stop.Station);    // earliest departure (filter's pick kept)
        }

        [Fact]
        public void Departure_BothGroups_ShortestSelectsLatestOriginAndEarliestDestination()
        {
            var (atKgx, atStp) = CreateLondonToManchesterOriginCandidates();

            var result = Selector(JourneyHeuristic.Shortest).ChooseDeparture(new[] { atKgx, atStp }, LondonOriginGroup(), ManchesterDestGroup());

            Assert.Equal(StPancras, result!.Stop.Stop.Station);                     // latest departure
            Assert.Equal(ManchesterOxfordRoad, result.FoundToStop.Stop.Station);    // earliest arrival
        }

        [Fact]
        public void Arrival_BothGroups_ShortestSelectsEarliestDestinationAndLatestOrigin()
        {
            var (atMco, atMcv, atMan) = CreateLondonToManchesterDestinationCandidates();

            var result = Selector(JourneyHeuristic.Shortest).ChooseArrival(new[] { atMco, atMcv, atMan }, LondonOriginGroup(), ManchesterDestGroup());

            Assert.Equal(ManchesterOxfordRoad, result!.Stop.Stop.Station);  // earliest arrival (path side)
            Assert.Equal(StPancras, result.FoundFromStop.Stop.Station);     // latest departure
        }

        [Fact]
        public void Departure_BothGroups_AppliesOriginAndDestinationPriorities()
        {
            var (atKgx, atStp) = CreateLondonToManchesterOriginCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseDeparture(
                new[] { atKgx, atStp }, LondonOriginGroup("STP"), ManchesterDestGroup("MCV"));

            Assert.Equal(StPancras, result!.Stop.Stop.Station);                  // origin priority wins selection
            Assert.Equal(ManchesterVictoria, result.FoundToStop.Stop.Station);   // dest priority overrides FoundToStop
        }

        [Fact]
        public void Arrival_BothGroups_AppliesDestinationAndOriginPriorities()
        {
            var (atMco, atMcv, atMan) = CreateLondonToManchesterDestinationCandidates();

            var result = Selector(JourneyHeuristic.Longest).ChooseArrival(
                new[] { atMco, atMcv, atMan }, LondonOriginGroup("STP"), ManchesterDestGroup("MCV"));

            Assert.Equal(ManchesterVictoria, result!.Stop.Stop.Station);  // dest priority wins selection (path side)
            Assert.Equal(StPancras, result.FoundFromStop.Stop.Station);   // origin priority overrides FoundFromStop
        }

        // ---------- Candidate builders ----------

        private static (ResolvedServiceStop atKgx, ResolvedServiceStop atStp) CreateLondonOriginCandidates()
        {
            var service = LondonAllToManchesterPiccadilly();
            var atKgx = new ResolvedServiceStop(service, service.Details.Locations[0]);
            var atStp = new ResolvedServiceStop(service, service.Details.Locations[1]);
            return (atKgx, atStp);
        }

        // Origin-side candidates for the both-groups case: gathered at each London member, filtered to the
        // Manchester destination group (so FoundToStop reflects the filter's natural backward-scan pick).
        private static (ResolvedServiceStop atKgx, ResolvedServiceStop atStp) CreateLondonToManchesterOriginCandidates()
        {
            var service = LondonAllToManchesterAll();
            var manchester = new HashSet<Station> { ManchesterPiccadilly, ManchesterVictoria, ManchesterOxfordRoad };
            var atKgx = new ResolvedServiceStop(service, service.Details.Locations[0]);
            atKgx.GoesToAnyOf(manchester);
            var atStp = new ResolvedServiceStop(service, service.Details.Locations[1]);
            atStp.GoesToAnyOf(manchester);
            return (atKgx, atStp);
        }

        // Destination-side candidates for the both-groups case: gathered at each Manchester member, filtered to
        // the London origin group (so FoundFromStop reflects the filter's natural forward-scan pick).
        private static (ResolvedServiceStop atMco, ResolvedServiceStop atMcv, ResolvedServiceStop atMan)
            CreateLondonToManchesterDestinationCandidates()
        {
            var service = LondonAllToManchesterAll();
            var london = new HashSet<Station> { KingsCross, StPancras };
            var atMco = new ResolvedServiceStop(service, service.Details.Locations[2]);
            atMco.ComesFromAnyOf(london);
            var atMcv = new ResolvedServiceStop(service, service.Details.Locations[3]);
            atMcv.ComesFromAnyOf(london);
            var atMan = new ResolvedServiceStop(service, service.Details.Locations[4]);
            atMan.ComesFromAnyOf(london);
            return (atMco, atMcv, atMan);
        }

        private static (ResolvedServiceStop atMco, ResolvedServiceStop atMcv, ResolvedServiceStop atMan)
            CreateManchesterDestinationCandidates()
        {
            var service = EustonToManchesterAll();
            var atMco = new ResolvedServiceStop(service, service.Details.Locations[1]);
            var atMcv = new ResolvedServiceStop(service, service.Details.Locations[2]);
            var atMan = new ResolvedServiceStop(service, service.Details.Locations[3]);
            return (atMco, atMcv, atMan);
        }
    }
}