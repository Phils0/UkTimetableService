using System;
using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class StationGroupStopOptimiserTest
    {
        // Manchester (destination) and London (origin) station fixtures, defined locally so the test
        // schedules can model the real groups discussed in planning, even though the test services' routes and
        // the test groups' priorities are contrived: GB@LO and GB@MA.
        // Each access returns a fresh Station to avoid LocationTimetable state leaking across tests.
        private static Station Euston              => MakeStation("EUS");
        private static Station KingsCross          => MakeStation("KGX");
        private static Station StPancras           => MakeStation("STP");
        private static Station ManchesterPiccadilly => MakeStation("MAN");
        private static Station ManchesterVictoria  => MakeStation("MCV");
        private static Station ManchesterOxfordRoad => MakeStation("MCO");

        private static Station MakeStation(string crs)
        {
            var s = new Station();
            s.AddMasterStationLocation(new Location { Tiploc = crs, ThreeLetterCode = crs });
            return s;
        }

        private static StationGroup LondonOriginGroup(params string[] priorities) =>
            new("GB@LO", new[] { "KGX", "STP" }, priorities.Length == 0 ? null : priorities);

        private static StationGroup ManchesterDestGroup(params string[] priorities) =>
            new("GB@MA", new[] { "MAN", "MCV", "MCO" }, priorities.Length == 0 ? null : priorities);

        // Euston (10:00) -> MCO (11:00) -> MCV (11:10) -> Manchester Piccadilly (11:20, destination)
        private static ResolvedService EustonToManchesterAll(string uid = "M12345")
        {
            var ten = TestSchedules.Ten;
            var stops = new[]
            {
                (ScheduleLocation)TestScheduleLocations.CreateOrigin(Euston, ten),
                TestScheduleLocations.CreateStop(ManchesterOxfordRoad, ten.AddMinutes(60)),
                TestScheduleLocations.CreateStop(ManchesterVictoria, ten.AddMinutes(70)),
                TestScheduleLocations.CreateDestination(ManchesterPiccadilly, ten.AddMinutes(80))
            };
            return TestSchedules.CreateService(timetableId: uid, stops: stops);
        }

        // King's Cross (10:00) -> St Pancras (10:05) -> Manchester Piccadilly (11:20, destination)
        // Contrived but useful: lets us test origin-group multiplicity when a service has public
        // departures at two London-area members in a row.
        private static ResolvedService LondonAllToManchesterPiccadilly(string uid = "L12345")
        {
            var ten = TestSchedules.Ten;
            var stops = new[]
            {
                (ScheduleLocation)TestScheduleLocations.CreateOrigin(KingsCross, ten),
                TestScheduleLocations.CreateStop(StPancras, ten.AddMinutes(5)),
                TestScheduleLocations.CreateDestination(ManchesterPiccadilly, ten.AddMinutes(80))
            };
            return TestSchedules.CreateService(timetableId: uid, stops: stops);
        }

        // King's Cross (10:00) -> St Pancras (10:05) -> MCO (11:00) -> MCV (11:10) -> Manchester Piccadilly (11:20)
        // Multiplicity on both ends: two London origins and three Manchester destinations.
        private static ResolvedService LondonAllToManchesterAll(string uid = "B12345")
        {
            var ten = TestSchedules.Ten;
            var stops = new[]
            {
                (ScheduleLocation)TestScheduleLocations.CreateOrigin(KingsCross, ten),
                TestScheduleLocations.CreateStop(StPancras, ten.AddMinutes(5)),
                TestScheduleLocations.CreateStop(ManchesterOxfordRoad, ten.AddMinutes(60)),
                TestScheduleLocations.CreateStop(ManchesterVictoria, ten.AddMinutes(70)),
                TestScheduleLocations.CreateDestination(ManchesterPiccadilly, ten.AddMinutes(80))
            };
            return TestSchedules.CreateService(timetableId: uid, stops: stops);
        }

        // ---------- OptimiseDepartures ----------

        [Fact]
        public void Departures_SingleCandidatePassesThrough()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var service = EustonToManchesterAll();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]); // Euston

            var result = optimiser.OptimiseDepartures(new[] { stop }, LondonOriginGroup(), null);

            Assert.Single(result);
            Assert.Same(stop, result[0]);
        }

        [Fact]
        public void Departures_OriginGroupPicksEarliestDeparture_WithLongestHeuristic()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atKgx, atStp) = CreateLondonOriginCandidates();

            var result = optimiser.OptimiseDepartures(new[] { atStp, atKgx }, LondonOriginGroup(), null);

            Assert.Single(result);
            Assert.Equal(KingsCross, result[0].Stop.Stop.Station);
        }

        [Fact]
        public void Departures_OriginGroupPicksLatestDeparture_WithShortestHeuristic()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Shortest);
            var (atKgx, atStp) = CreateLondonOriginCandidates();

            var result = optimiser.OptimiseDepartures(new[] { atKgx, atStp }, LondonOriginGroup(), null);

            Assert.Single(result);
            Assert.Equal(StPancras, result[0].Stop.Stop.Station);
        }

        [Fact]
        public void Departures_OriginPriority_WinsOverHeuristic()
        {
            // Longest alone would pick KGX (earliest); priorities=["STP"] forces St Pancras.
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atKgx, atStp) = CreateLondonOriginCandidates();

            var result = optimiser.OptimiseDepartures(new[] { atKgx, atStp }, LondonOriginGroup("STP"), null);

            Assert.Single(result);
            Assert.Equal(StPancras, result[0].Stop.Stop.Station);
        }

        [Fact]
        public void Departures_OriginPriorityMiss_FallsBackToHeuristic()
        {
            // Priority "EUS" is not an origin in this service; fall back to Longest -> KGX.
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atKgx, atStp) = CreateLondonOriginCandidates();

            var result = optimiser.OptimiseDepartures(new[] { atKgx, atStp }, LondonOriginGroup("EUS"), null);

            Assert.Single(result);
            Assert.Equal(KingsCross, result[0].Stop.Stop.Station);
        }

        [Fact]
        public void Departures_DestinationOverride_AppliesPriority()
        {
            // The filter's natural backward scan picks MAN (latest Manchester arrival).
            // priorities=["MCV"] must override that to Manchester Victoria.
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var service = EustonToManchesterAll();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            stop.GoesTo(ManchesterPiccadilly); // simulate filter

            var result = optimiser.OptimiseDepartures(new[] { stop }, null, ManchesterDestGroup("MCV"));

            Assert.Single(result);
            Assert.Equal(ManchesterVictoria, result[0].FoundToStop.Stop.Station);
        }

        [Fact]
        public void Departures_DestinationOverrideShortest_PicksEarliestArrival()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Shortest);
            var service = EustonToManchesterAll();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            stop.GoesTo(ManchesterPiccadilly);

            var result = optimiser.OptimiseDepartures(new[] { stop }, null, ManchesterDestGroup());

            Assert.Single(result);
            Assert.Equal(ManchesterOxfordRoad, result[0].FoundToStop.Stop.Station);
        }

        [Fact]
        public void Departures_DestinationOverrideIsNoOp_ForLongestWithoutPriorities()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var service = EustonToManchesterAll();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);
            stop.GoesTo(ManchesterPiccadilly);
            var preOverride = stop.FoundToStop;

            var result = optimiser.OptimiseDepartures(new[] { stop }, null, ManchesterDestGroup());

            Assert.Same(preOverride, result[0].FoundToStop);
        }

        // ---------- OptimiseArrivals ----------

        [Fact]
        public void Arrivals_DestinationGroupPicksLatestArrival_WithLongestHeuristic()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atMco, atMcv, atMan) = CreateManchesterDestinationCandidates();

            var result = optimiser.OptimiseArrivals(new[] { atMco, atMcv, atMan }, null, ManchesterDestGroup());

            Assert.Single(result);
            Assert.Equal(ManchesterPiccadilly, result[0].Stop.Stop.Station);
        }

        [Fact]
        public void Arrivals_DestinationGroupPicksEarliestArrival_WithShortestHeuristic()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Shortest);
            var (atMco, atMcv, atMan) = CreateManchesterDestinationCandidates();

            var result = optimiser.OptimiseArrivals(new[] { atMan, atMcv, atMco }, null, ManchesterDestGroup());

            Assert.Single(result);
            Assert.Equal(ManchesterOxfordRoad, result[0].Stop.Stop.Station);
        }

        [Fact]
        public void Arrivals_DestinationPriority_WinsOverHeuristic()
        {
            // Longest alone would pick MAN; priorities=["MCV"] forces Manchester Victoria.
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atMco, atMcv, atMan) = CreateManchesterDestinationCandidates();

            var result = optimiser.OptimiseArrivals(new[] { atMco, atMcv, atMan }, null, ManchesterDestGroup("MCV"));

            Assert.Single(result);
            Assert.Equal(ManchesterVictoria, result[0].Stop.Stop.Station);
        }

        [Fact]
        public void Arrivals_OriginOverride_AppliesPriority()
        {
            // Single candidate arriving at Manchester from London. Filter's natural pick (Longest = earliest
            // departure) would be KGX; priorities=["STP"] must override to St Pancras.
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var service = LondonAllToManchesterPiccadilly();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[2]); // Manchester
            stop.ComesFrom(KingsCross);

            var result = optimiser.OptimiseArrivals(new[] { stop }, LondonOriginGroup("STP"), null);

            Assert.Single(result);
            Assert.Equal(StPancras, result[0].FoundFromStop.Stop.Station);
        }

        [Fact]
        public void Arrivals_OriginOverride_ShortestPicksLatestDeparture()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Shortest);
            var service = LondonAllToManchesterPiccadilly();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[2]);
            stop.ComesFrom(KingsCross);

            var result = optimiser.OptimiseArrivals(new[] { stop }, LondonOriginGroup(), null);

            Assert.Single(result);
            Assert.Equal(StPancras, result[0].FoundFromStop.Stop.Station);
        }

        // ---------- Both path and query are groups ----------

        [Fact]
        public void Departures_BothGroups_LongestSelectsEarliestOriginAndKeepsLatestDestination()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atKgx, atStp) = CreateLondonToManchesterOriginCandidates();

            var result = optimiser.OptimiseDepartures(new[] { atStp, atKgx }, LondonOriginGroup(), ManchesterDestGroup());

            Assert.Single(result);
            Assert.Equal(KingsCross, result[0].Stop.Stop.Station);                  // earliest departure
            Assert.Equal(ManchesterPiccadilly, result[0].FoundToStop.Stop.Station); // latest arrival (filter's pick kept)
        }

        [Fact]
        public void Arrivals_BothGroups_LongestSelectsLatestDestinationAndKeepsEarliestOrigin()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atMco, atMcv, atMan) = CreateLondonToManchesterDestinationCandidates();

            var result = optimiser.OptimiseArrivals(new[] { atMco, atMcv, atMan }, LondonOriginGroup(), ManchesterDestGroup());

            Assert.Single(result);
            Assert.Equal(ManchesterPiccadilly, result[0].Stop.Stop.Station);    // latest arrival (path side)
            Assert.Equal(KingsCross, result[0].FoundFromStop.Stop.Station);     // earliest departure (filter's pick kept)
        }

        [Fact]
        public void Departures_BothGroups_ShortestSelectsLatestOriginAndEarliestDestination()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Shortest);
            var (atKgx, atStp) = CreateLondonToManchesterOriginCandidates();

            var result = optimiser.OptimiseDepartures(new[] { atKgx, atStp }, LondonOriginGroup(), ManchesterDestGroup());

            Assert.Single(result);
            Assert.Equal(StPancras, result[0].Stop.Stop.Station);                   // latest departure
            Assert.Equal(ManchesterOxfordRoad, result[0].FoundToStop.Stop.Station); // earliest arrival
        }

        [Fact]
        public void Arrivals_BothGroups_ShortestSelectsEarliestDestinationAndLatestOrigin()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Shortest);
            var (atMco, atMcv, atMan) = CreateLondonToManchesterDestinationCandidates();

            var result = optimiser.OptimiseArrivals(new[] { atMco, atMcv, atMan }, LondonOriginGroup(), ManchesterDestGroup());

            Assert.Single(result);
            Assert.Equal(ManchesterOxfordRoad, result[0].Stop.Stop.Station);    // earliest arrival (path side)
            Assert.Equal(StPancras, result[0].FoundFromStop.Stop.Station);      // latest departure
        }

        [Fact]
        public void Departures_BothGroups_AppliesOriginAndDestinationPriorities()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atKgx, atStp) = CreateLondonToManchesterOriginCandidates();

            var result = optimiser.OptimiseDepartures(
                new[] { atKgx, atStp }, LondonOriginGroup("STP"), ManchesterDestGroup("MCV"));

            Assert.Single(result);
            Assert.Equal(StPancras, result[0].Stop.Stop.Station);                 // origin priority wins selection
            Assert.Equal(ManchesterVictoria, result[0].FoundToStop.Stop.Station); // dest priority overrides FoundToStop
        }

        [Fact]
        public void Arrivals_BothGroups_AppliesDestinationAndOriginPriorities()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var (atMco, atMcv, atMan) = CreateLondonToManchesterDestinationCandidates();

            var result = optimiser.OptimiseArrivals(
                new[] { atMco, atMcv, atMan }, LondonOriginGroup("STP"), ManchesterDestGroup("MCV"));

            Assert.Single(result);
            Assert.Equal(ManchesterVictoria, result[0].Stop.Stop.Station);    // dest priority wins selection (path side)
            Assert.Equal(StPancras, result[0].FoundFromStop.Stop.Station);    // origin priority overrides FoundFromStop
        }

        // ---------- Edge cases ----------

        [Fact]
        public void EmptyInput_ReturnsEmpty()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);

            var result = optimiser.OptimiseDepartures(Array.Empty<ResolvedServiceStop>(), LondonOriginGroup(), null);

            Assert.Empty(result);
        }

        [Fact]
        public void DifferentServices_AreKeptSeparate()
        {
            var optimiser = new StationGroupStopOptimiser(JourneyHeuristic.Longest);
            var serviceA = LondonAllToManchesterPiccadilly(uid: "L11111");
            var serviceB = LondonAllToManchesterPiccadilly(uid: "L22222");
            var atKgxA = new ResolvedServiceStop(serviceA, serviceA.Details.Locations[0]);
            var atKgxB = new ResolvedServiceStop(serviceB, serviceB.Details.Locations[0]);

            var result = optimiser.OptimiseDepartures(new[] { atKgxA, atKgxB }, LondonOriginGroup(), null);

            Assert.Equal(2, result.Length);
        }

        // ---------- Helpers ----------

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