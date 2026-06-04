using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Controllers;
using Timetable.Web.Model;
using Xunit;
using static Timetable.Web.Test.Controllers.StationGroupControllerTestData;

namespace Timetable.Web.Test.Controllers
{
    /// <summary>
    /// Station-group behaviour of <see cref="DeparturesController"/>: a group code in the path iterates its members;
    /// a group code in ?to= becomes a multi-station filter; the optimiser collapses a service calling at several
    /// members to one row. Plain-CRS behaviour is asserted unchanged in <see cref="DeparturesControllerTest"/>.
    /// </summary>
    public class DeparturesControllerStationGroupsTest
    {

        [Fact]
        public async Task GroupOriginIteratesEveryMemberAndOptimises()
        {
            var group = Manchester;
            var data = StubData();
            var optimiser = PassThroughOptimiser();
            var controller = CreateController(data, Lookup(group), optimiser);

            var response = await controller.Departures("GB@MA", Aug12AtTen) as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            foreach (var member in group.Members)
                data.Received().FindDepartures(member.ThreeLetterCode, Aug12AtTen, Arg.Any<GatherConfiguration>());
            optimiser.Received(1).OptimiseDepartures(
                Arg.Any<IEnumerable<ResolvedServiceStop>>(),
                Arg.Is<StationGroup>(g => g != null && g.Code == "GB@MA"),
                Arg.Any<StationGroup>());
        }

        [Fact]
        public async Task GroupDestinationUsesMultiStationFilterAndOptimises()
        {
            var data = StubData();
            data.TryGetStation("EUS", out Arg.Any<Station>()).Returns(true);
            var optimiser = PassThroughOptimiser();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.DeparturesGoTo(Arg.Any<IReadOnlySet<Station>>()).Returns(GatherFilterFactory.NoFilter);

            var controller = CreateController(data, Lookup(Manchester), optimiser, filterFactory);
            var response = await controller.Departures("EUS", Aug12AtTen, to: "GB@MA") as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            filterFactory.Received().DeparturesGoTo(Arg.Any<IReadOnlySet<Station>>());
            optimiser.Received(1).OptimiseDepartures(
                Arg.Any<IEnumerable<ResolvedServiceStop>>(),
                Arg.Any<StationGroup>(),
                Arg.Is<StationGroup>(g => g != null && g.Code == "GB@MA"));
        }

        [Fact]
        public async Task SameGroupOnBothSidesIsPermitted()
        {
            var data = StubData();
            var optimiser = PassThroughOptimiser();
            var controller = CreateController(data, Lookup(London), optimiser);

            var response = await controller.Departures("GB@LO", Aug12AtTen, to: "GB@LO") as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            optimiser.Received(1).OptimiseDepartures(
                Arg.Any<IEnumerable<ResolvedServiceStop>>(),
                Arg.Is<StationGroup>(g => g != null && g.Code == "GB@LO"),
                Arg.Is<StationGroup>(g => g != null && g.Code == "GB@LO"));
        }

        [Fact]
        public async Task UnknownGroupInPathReturnsNotFound()
        {
            // Neither CRS nor group resolves; the find delegate runs against the raw code and the data layer reports
            // LocationNotFound - the same 404 model as an unknown CRS.
            var data = StubData(status: FindStatus.LocationNotFound, stops: Array.Empty<ResolvedServiceStop>());
            var optimiser = Substitute.For<IStationGroupStopOptimiser>();
            var controller = CreateController(data, Lookup(Manchester), optimiser);

            var response = await controller.Departures("GB@ZZ", Aug12AtTen) as ObjectResult;

            Assert.Equal(404, response.StatusCode);
            optimiser.DidNotReceive().OptimiseDepartures(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>());
        }

        [Fact]
        public async Task UnknownGroupInQueryWarnsAndDropsFilter()
        {
            var data = StubData();
            data.TryGetStation("EUS", out Arg.Any<Station>()).Returns(true);
            var optimiser = Substitute.For<IStationGroupStopOptimiser>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);

            var controller = CreateController(data, Lookup(Manchester), optimiser, filterFactory);
            var response = await controller.Departures("EUS", Aug12AtTen, to: "GB@ZZ") as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            filterFactory.DidNotReceive().DeparturesGoTo(Arg.Any<Station>());
            filterFactory.DidNotReceive().DeparturesGoTo(Arg.Any<IReadOnlySet<Station>>());
            optimiser.DidNotReceive().OptimiseDepartures(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>());
        }

        [Fact]
        public async Task PlainCrsDoesNotInvokeOptimiser()
        {
            var data = StubData();
            data.TryGetStation("SUR", out Arg.Any<Station>()).Returns(true);
            var optimiser = Substitute.For<IStationGroupStopOptimiser>();
            var controller = CreateController(data, Lookup(Manchester), optimiser);

            var response = await controller.Departures("SUR", Aug12AtTen) as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            optimiser.DidNotReceive().OptimiseDepartures(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>());
        }

        [Fact]
        public async Task CancelledServicesAreFilteredBeforeTheOptimiserRuns()
        {
            // Pins the dedup -> optimise ordering: the cancelled-overlay filter must run upstream so the optimiser
            // only ever sees the surviving (non-cancelled) rows.
            var live = TestSchedules.CreateResolvedDepartureStop(timetableId: "X10001");
            var cancelled = TestSchedules.CreateResolvedDepartureStop(timetableId: "X10002", isCancelled: true);
            var data = StubData(stops: new[] { live, cancelled });
            var optimiser = PassThroughOptimiser();
            var group = new StationGroup("GB@SM", new[] { TestStations.Create("EUS") });
            var controller = CreateController(data, Lookup(group), optimiser);

            await controller.Departures("GB@SM", Aug12AtTen);

            optimiser.Received(1).OptimiseDepartures(
                Arg.Is<IEnumerable<ResolvedServiceStop>>(s => s.All(x => !x.Service.IsCancelled) && s.Count() == 1),
                Arg.Any<StationGroup>(),
                Arg.Any<StationGroup>());
        }

        [Theory]
        [InlineData("GB@LO")] // @ - percent-encoded as %40
        [InlineData("GB#RE")] // # - percent-encoded as %23
        [InlineData("GB^PH")] // ^ - percent-encoded as %5E
        public async Task GroupCodesWithReservedUrlSymbolsResolveOnceDecoded(string code)
        {
            // ASP.NET decodes the reserved symbol before the action runs, so the controller receives the canonical
            // code. This pins that nothing re-reads the raw, still-encoded path/query for any of the delimiters we use.
            var group = new StationGroup(code, new[] { TestStations.Create("AAA"), TestStations.Create("BBB") });
            var data = StubData();
            var optimiser = PassThroughOptimiser();
            var controller = CreateController(data, Lookup(group), optimiser);

            var response = await controller.Departures(code, Aug12AtTen) as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            optimiser.Received(1).OptimiseDepartures(
                Arg.Any<IEnumerable<ResolvedServiceStop>>(),
                Arg.Is<StationGroup>(g => g != null && g.Code == code),
                Arg.Any<StationGroup>());
        }

        [Fact]
        public async Task WindowedGroupResultsAreReWindowedAroundThePivot()
        {
            // A group origin gathers one window per member, so the merged set spans both sides of the pivot. With
            // before=1/after=1 and four distinct services at 09:40, 09:55, 10:10, 10:20 the result must be the latest
            // before 10:00 (09:55) and the earliest at/after (10:10) - not simply the first two by time.
            var stops = new[]
            {
                StopDepartingAt("X10001", TestSchedules.NineForty),
                StopDepartingAt("X10002", TestSchedules.NineFiftyFive),
                StopDepartingAt("X10003", TestSchedules.TenTen),
                StopDepartingAt("X10004", TestSchedules.TenTwenty),
            };
            var data = StubData(stops: stops);
            var optimiser = new StationGroupStopOptimiser(new CanonicalStopSelector(JourneyHeuristic.Longest), Substitute.For<ILogger>());
            var controller = CreateController(data, Lookup(London), optimiser);

            var response = await controller.Departures("GB@LO", Aug12AtTen, before: 1, after: 1) as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            var services = response.Value as FoundSummaryResponse;
            Assert.Equal(new[] { "X10002", "X10003" }, services.Services.Select(s => s.Service.TimetableUid).OrderBy(uid => uid));
        }

        private static ResolvedServiceStop StopDepartingAt(string timetableId, Time origin) =>
            TestSchedules.CreateResolvedDepartureStop(timetableId: timetableId, stops: TestSchedules.CreateThreeStopSchedule(origin));

        private static DeparturesController CreateController(ILocationData data, StationGroupLookup groups,
            IStationGroupStopOptimiser optimiser, IFilterFactory? filters = null) =>
            new DeparturesController(data, filters ?? FilterFactory, groups, new GroupSearchOrchestrator(Substitute.For<ILogger>()), optimiser, Mapper, Substitute.For<ILogger>());
    }
}