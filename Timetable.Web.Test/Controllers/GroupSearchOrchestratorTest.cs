using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Controllers;
using Xunit;
using static Timetable.Web.Test.Controllers.StationGroupControllerTestData;

namespace Timetable.Web.Test.Controllers
{
    public class GroupSearchOrchestratorTest
    {
        private static readonly DateTime Aug12AtTen = new DateTime(2019, 8, 12, 10, 0, 0);
        private static readonly DateTime Aug12AtElevenPm = new DateTime(2019, 8, 12, 23, 0, 0);

        // ----- ReWindow (via the built transform) -----

        [Fact]
        public void ReWindowKeepsTheServicesEitherSideOfThePivot()
        {
            var stops = new[]
            {
                DepartingAt("X10001", 9, 40), DepartingAt("X10002", 9, 55),
                DepartingAt("X10003", 10, 10), DepartingAt("X10004", 10, 20),
            };

            var result = WindowedDepartures(Aug12AtTen, before: 1, after: 1)(stops);

            Assert.Equal(new[] { "X10002", "X10003" }, Uids(result));
        }

        [Fact]
        public void ReWindowTakesTheLatestServicesWhenAllAreBeforeThePivot()
        {
            var stops = new[]
            {
                DepartingAt("X10001", 9, 30), DepartingAt("X10002", 9, 40), DepartingAt("X10003", 9, 50),
            };

            var result = WindowedDepartures(Aug12AtTen, before: 2, after: 2)(stops);

            Assert.Equal(new[] { "X10002", "X10003" }, Uids(result));
        }

        [Fact]
        public void ReWindowTakesTheEarliestServicesWhenAllAreAtOrAfterThePivot()
        {
            var stops = new[]
            {
                DepartingAt("X10001", 10, 10), DepartingAt("X10002", 10, 20), DepartingAt("X10003", 10, 30),
            };

            var result = WindowedDepartures(Aug12AtTen, before: 2, after: 2)(stops);

            Assert.Equal(new[] { "X10001", "X10002" }, Uids(result));
        }

        [Fact]
        public void ReWindowReturnsAtLeastOneServiceWhenBeforeAndAfterAreBothZero()
        {
            var stops = new[] { DepartingAt("X10001", 9, 50), DepartingAt("X10002", 10, 10) };

            var result = WindowedDepartures(Aug12AtTen, before: 0, after: 0)(stops);

            Assert.Equal(new[] { "X10002" }, Uids(result)); // guard promotes after to 1: earliest at/after the pivot
        }

        [Fact]
        public void ReWindowOrdersByAbsoluteInstantAcrossMidnight()
        {
            // Pivot 23:00; the 00:10 service is the *next day* (held as 24:10), so it is later than 23:30, not earlier.
            // Clock-only ordering would wrongly treat it as 00:10 and surface it over the 23:30 service.
            var stops = new[]
            {
                DepartingAt("X10001", 22, 30),
                DepartingAt("X10002", 23, 30),
                DepartingAt("X10003", 24, 10),
            };

            var result = WindowedDepartures(Aug12AtElevenPm, before: 1, after: 1)(stops);

            Assert.Equal(new[] { "X10001", "X10002" }, Uids(result));
        }

        [Fact]
        public void BuildOptimiseIsTheIdentityWhenNeitherSideIsAGroup()
        {
            var optimiser = PassThroughOptimiser();
            var orchestrator = new GroupSearchOrchestrator(new DeparturesDirection(optimiser), Substitute.For<ILogger>());
            var stops = new[] { DepartingAt("X10001", 10, 10) };

            var result = orchestrator.BuildOptimise(pathGroup: null, queryGroup: null,
                pivot: Aug12AtTen, before: 1, after: 1)(stops);

            Assert.Same(stops, result);
            optimiser.DidNotReceive().OptimiseDepartures(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>());
        }

        // ----- GatherAcrossGroupMembers -----

        [Fact]
        public void GatherCollectsSuccessfulMembersAndWarnsOnAMemberTheTimetableCannotFind()
        {
            var logger = Substitute.For<ILogger>();
            var orchestrator = new GroupSearchOrchestrator(new DeparturesDirection(PassThroughOptimiser()), logger);
            var group = new StationGroup("GB@MA", new[] { TestStations.Create("MAN"), TestStations.Create("MCV"), TestStations.Create("MCO") });

            var (status, services) = orchestrator.GatherAcrossGroupMembers(group, member => member switch
            {
                "MAN" => (FindStatus.Success, new[] { DepartingAt("X10001", 10, 10) }),
                "MCV" => (FindStatus.LocationNotFound, Array.Empty<ResolvedServiceStop>()),
                _ => (FindStatus.NoServicesForLocation, Array.Empty<ResolvedServiceStop>()),
            });

            Assert.Equal(FindStatus.Success, status);
            Assert.Equal(new[] { "X10001" }, Uids(services));
            // Exactly one warning: the not-found member. The empty (NoServicesForLocation) member stays silent.
            logger.Received(1).Warning(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FindStatus>());
        }

        [Fact]
        public void GatherReturnsNoServicesAndStaysSilentWhenEveryMemberIsSimplyEmpty()
        {
            var logger = Substitute.For<ILogger>();
            var orchestrator = new GroupSearchOrchestrator(new DeparturesDirection(PassThroughOptimiser()), logger);
            var group = new StationGroup("GB@MA", new[] { TestStations.Create("MAN"), TestStations.Create("MCV") });

            var (status, services) = orchestrator.GatherAcrossGroupMembers(group,
                _ => (FindStatus.NoServicesForLocation, Array.Empty<ResolvedServiceStop>()));

            Assert.Equal(FindStatus.NoServicesForLocation, status);
            Assert.Empty(services);
            logger.DidNotReceive().Warning(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FindStatus>());
        }

        [Fact]
        public void GatherSurfacesAnErrorMemberAsErrorWhenNoneSucceed()
        {
            // No member has services, but one errored: return Error (-> 500) rather than masking it as a 404
            // NoServicesForLocation. Error outranks the other members' statuses.
            var orchestrator = new GroupSearchOrchestrator(new DeparturesDirection(PassThroughOptimiser()), Substitute.For<ILogger>());
            var group = new StationGroup("GB@MA", new[] { TestStations.Create("MAN"), TestStations.Create("MCV") });

            var (status, _) = orchestrator.GatherAcrossGroupMembers(group, member => member switch
            {
                "MAN" => (FindStatus.Error, Array.Empty<ResolvedServiceStop>()),
                _ => (FindStatus.NoServicesForLocation, Array.Empty<ResolvedServiceStop>()),
            });

            Assert.Equal(FindStatus.Error, status);
        }

        [Fact]
        public void GatherPrefersLocationNotFoundOverNoServicesWhenNoneSucceed()
        {
            var orchestrator = new GroupSearchOrchestrator(new DeparturesDirection(PassThroughOptimiser()), Substitute.For<ILogger>());
            var group = new StationGroup("GB@MA", new[] { TestStations.Create("MAN"), TestStations.Create("MCV") });

            var (status, _) = orchestrator.GatherAcrossGroupMembers(group, member => member switch
            {
                "MAN" => (FindStatus.LocationNotFound, Array.Empty<ResolvedServiceStop>()),
                _ => (FindStatus.NoServicesForLocation, Array.Empty<ResolvedServiceStop>()),
            });

            Assert.Equal(FindStatus.LocationNotFound, status);
        }

        // ----- helpers -----

        // The transform for a path-side (origin) group departures search, ordered/windowed by origin departure time.
        private static Func<ResolvedServiceStop[], ResolvedServiceStop[]> WindowedDepartures(DateTime pivot, ushort before, ushort after)
        {
            var orchestrator = new GroupSearchOrchestrator(new DeparturesDirection(PassThroughOptimiser()), Substitute.For<ILogger>());
            return orchestrator.BuildOptimise(pathGroup: London, queryGroup: null, pivot: pivot, before, after);
        }

        // A departure stop whose origin (Surbiton) departs at the given time-of-day on the test date; hours >= 24 land
        // it on the next day, mirroring how a past-midnight service is held.
        private static ResolvedServiceStop DepartingAt(string timetableId, int hours, int minutes) =>
            TestSchedules.CreateResolvedDepartureStop(
                timetableId: timetableId,
                stops: TestSchedules.CreateThreeStopSchedule(new Time(new TimeSpan(hours, minutes, 0))));

        private static IEnumerable<string> Uids(IEnumerable<ResolvedServiceStop> stops) =>
            stops.Select(s => s.Service.TimetableUid);
    }
}