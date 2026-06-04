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
    /// Station-group behaviour of <see cref="ArrivalsController"/>. For arrivals the path parameter is the
    /// destination (iterated when it is a group) and ?from= is the origin (a multi-station filter when it is a
    /// group). The mirror of <see cref="DeparturesControllerStationGroupsTest"/>; plain-CRS behaviour is asserted
    /// unchanged in <see cref="ArrivalsControllerTest"/>.
    /// </summary>
    public class ArrivalsControllerStationGroupsTest
    {
        [Fact]
        public async Task GroupDestinationIteratesEveryMemberAndOptimises()
        {
            var group = Manchester;
            var data = StubData();
            var optimiser = PassThroughOptimiser();
            var controller = CreateController(data, Lookup(group), optimiser);

            var response = await controller.Arrivals("GB@MA", Aug12AtTen) as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            foreach (var member in group.Members)
                data.Received().FindArrivals(member.ThreeLetterCode, Aug12AtTen, Arg.Any<GatherConfiguration>());
            optimiser.Received(1).OptimiseArrivals(
                Arg.Any<IEnumerable<ResolvedServiceStop>>(),
                Arg.Any<StationGroup>(),
                Arg.Is<StationGroup>(g => g != null && g.Code == "GB@MA"));
        }

        [Fact]
        public async Task GroupOriginUsesMultiStationFilterAndOptimises()
        {
            var data = StubData();
            data.TryGetStation("EUS", out Arg.Any<Station>()).Returns(true);
            var optimiser = PassThroughOptimiser();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ArrivalsComeFrom(Arg.Any<IReadOnlySet<Station>>()).Returns(GatherFilterFactory.NoFilter);

            var controller = CreateController(data, Lookup(Manchester), optimiser, filterFactory);
            var response = await controller.Arrivals("EUS", Aug12AtTen, from: "GB@MA") as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            filterFactory.Received().ArrivalsComeFrom(Arg.Any<IReadOnlySet<Station>>());
            optimiser.Received(1).OptimiseArrivals(
                Arg.Any<IEnumerable<ResolvedServiceStop>>(),
                Arg.Is<StationGroup>(g => g != null && g.Code == "GB@MA"),
                Arg.Any<StationGroup>());
        }

        [Fact]
        public async Task SameGroupOnBothSidesIsPermitted()
        {
            var data = StubData();
            var optimiser = PassThroughOptimiser();
            var controller = CreateController(data, Lookup(London), optimiser);

            var response = await controller.Arrivals("GB@LO", Aug12AtTen, from: "GB@LO") as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            optimiser.Received(1).OptimiseArrivals(
                Arg.Any<IEnumerable<ResolvedServiceStop>>(),
                Arg.Is<StationGroup>(g => g != null && g.Code == "GB@LO"),
                Arg.Is<StationGroup>(g => g != null && g.Code == "GB@LO"));
        }

        [Fact]
        public async Task UnknownGroupInPathReturnsNotFound()
        {
            var data = StubData(status: FindStatus.LocationNotFound, stops: Array.Empty<ResolvedServiceStop>());
            var optimiser = Substitute.For<IStationGroupStopOptimiser>();
            var controller = CreateController(data, Lookup(Manchester), optimiser);

            var response = await controller.Arrivals("GB@ZZ", Aug12AtTen) as ObjectResult;

            Assert.Equal(404, response.StatusCode);
            optimiser.DidNotReceive().OptimiseArrivals(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>());
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
            var response = await controller.Arrivals("EUS", Aug12AtTen, from: "GB@ZZ") as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            filterFactory.DidNotReceive().ArrivalsComeFrom(Arg.Any<Station>());
            filterFactory.DidNotReceive().ArrivalsComeFrom(Arg.Any<IReadOnlySet<Station>>());
            optimiser.DidNotReceive().OptimiseArrivals(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>());
        }

        [Fact]
        public async Task PlainCrsDoesNotInvokeOptimiser()
        {
            var data = StubData();
            data.TryGetStation("CLJ", out Arg.Any<Station>()).Returns(true);
            var optimiser = Substitute.For<IStationGroupStopOptimiser>();
            var controller = CreateController(data, Lookup(Manchester), optimiser);

            var response = await controller.Arrivals("CLJ", Aug12AtTen) as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            optimiser.DidNotReceive().OptimiseArrivals(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>());
        }

        [Fact]
        public async Task CancelledServicesAreFilteredBeforeTheOptimiserRuns()
        {
            // Pins the dedup -> optimise ordering: the cancelled-overlay filter must run upstream so the optimiser
            // only ever sees the surviving (non-cancelled) rows.
            var live = TestSchedules.CreateResolvedArrivalStop(timetableId: "X10001");
            var cancelled = TestSchedules.CreateResolvedArrivalStop(timetableId: "X10002", isCancelled: true);
            var data = StubData(stops: new[] { live, cancelled });
            var optimiser = PassThroughOptimiser();
            var group = new StationGroup("GB@SM", new[] { TestStations.Create("EUS") });
            var controller = CreateController(data, Lookup(group), optimiser);

            await controller.Arrivals("GB@SM", Aug12AtTen);

            optimiser.Received(1).OptimiseArrivals(
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

            var response = await controller.Arrivals(code, Aug12AtTen) as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            optimiser.Received(1).OptimiseArrivals(
                Arg.Any<IEnumerable<ResolvedServiceStop>>(),
                Arg.Any<StationGroup>(),
                Arg.Is<StationGroup>(g => g != null && g.Code == code));
        }

        [Fact]
        public async Task WindowedGroupResultsAreReWindowedAroundThePivot()
        {
            // A group destination gathers one window per member, so the merged set spans both sides of the pivot. With
            // before=1/after=1 and four distinct services arriving 09:40, 09:55, 10:10, 10:20 the result must be the
            // latest before 10:00 (09:55) and the earliest at/after (10:10) - not simply the first two by time.
            var stops = new[]
            {
                StopArrivingAt("X10001", TestSchedules.NineForty),
                StopArrivingAt("X10002", TestSchedules.NineFiftyFive),
                StopArrivingAt("X10003", TestSchedules.TenTen),
                StopArrivingAt("X10004", TestSchedules.TenTwenty),
            };
            var data = StubData(stops: stops);
            var optimiser = new StationGroupStopOptimiser(new CanonicalStopSelector(JourneyHeuristic.Longest), Substitute.For<ILogger>());
            var controller = CreateController(data, Lookup(London), optimiser);

            var response = await controller.Arrivals("GB@LO", Aug12AtTen, before: 1, after: 1) as ObjectResult;

            Assert.Equal(200, response.StatusCode);
            var services = response.Value as FoundSummaryResponse;
            Assert.Equal(new[] { "X10002", "X10003" }, services.Services.Select(s => s.Service.TimetableUid).OrderBy(uid => uid));
        }

        // Destination arrival in the three-stop schedule is start + 30 min, so offset the schedule start to land the
        // arrival on the wanted time.
        private static ResolvedServiceStop StopArrivingAt(string timetableId, Time arrival) =>
            TestSchedules.CreateResolvedArrivalStop(timetableId: timetableId, stops: TestSchedules.CreateThreeStopSchedule(arrival.AddMinutes(-30)));

        private static ArrivalsController CreateController(ILocationData data, StationGroupLookup groups,
            IStationGroupStopOptimiser optimiser, IFilterFactory? filters = null) =>
            new ArrivalsController(data, filters ?? FilterFactory, groups, new GroupSearchOrchestrator(Substitute.For<ILogger>()), optimiser, Mapper, Substitute.For<ILogger>());
    }
}