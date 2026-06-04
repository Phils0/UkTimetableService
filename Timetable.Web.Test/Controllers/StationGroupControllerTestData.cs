using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Mapping;

namespace Timetable.Web.Test.Controllers
{
    /// <summary>
    /// Direction-agnostic fixtures shared by <see cref="DeparturesControllerStationGroupsTest"/> and
    /// <see cref="ArrivalsControllerStationGroupsTest"/>.
    /// </summary>
    internal static class StationGroupControllerTestData
    {
        public static readonly DateTime Aug12AtTen = new DateTime(2019, 8, 12, 10, 0, 0);

        // Only the codes and membership matter to the controller-dispatch assertions; member-stop content is
        // irrelevant because the optimiser is a substitute in most tests. Fresh instances per access (the
        // TestStations contract) so nothing leaks between tests.
        public static StationGroup London => new StationGroup("GB@LO", new[] { TestStations.Create("EUS"), TestStations.Create("KGX") });
        public static StationGroup Manchester => new StationGroup("GB@MA", new[] { TestStations.Create("MAN"), TestStations.Create("MCV"), TestStations.Create("MCO") }, new[] { TestStations.Create("MAN") });

        public static StationGroupLookup Lookup(params StationGroup[] groups) =>
            new StationGroupLookup(groups.ToDictionary(g => g.Code, g => g, StringComparer.OrdinalIgnoreCase));

        public static GatherFilterFactory FilterFactory => new GatherFilterFactory(Substitute.For<ILogger>());

        public static readonly IMapper Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ToViewModelProfile>()).CreateMapper();

        /// <summary>
        /// A stub <see cref="ILocationData"/> wired so every find entry-point (departures/arrivals, windowed/full-day)
        /// returns the same <paramref name="stops"/> and <paramref name="status"/>.
        /// </summary>
        public static ILocationData StubData(ResolvedServiceStop[] stops = null, FindStatus status = FindStatus.Success)
        {
            stops = stops ?? new[] { TestSchedules.CreateResolvedDepartureStop() };
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((status, stops));
            data.AllDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration.GatherFilter>(), Arg.Any<Time>())
                .Returns((status, stops));
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((status, stops));
            data.AllArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration.GatherFilter>(), Arg.Any<Time>())
                .Returns((status, stops));
            data.Filters.Returns(Filters.Instance);
            return data;
        }

        /// <summary>
        /// A substitute optimiser whose departures and arrivals entry-points both return their input unchanged, so the
        /// surrounding dispatch/windowing can be asserted without the optimiser's own collapse logic interfering.
        /// </summary>
        public static IStationGroupStopOptimiser PassThroughOptimiser()
        {
            var optimiser = Substitute.For<IStationGroupStopOptimiser>();
            optimiser.OptimiseDepartures(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>())
                .Returns(ci => ((IEnumerable<ResolvedServiceStop>)ci[0]).ToArray());
            optimiser.OptimiseArrivals(Arg.Any<IEnumerable<ResolvedServiceStop>>(), Arg.Any<StationGroup>(), Arg.Any<StationGroup>())
                .Returns(ci => ((IEnumerable<ResolvedServiceStop>)ci[0]).ToArray());
            return optimiser;
        }
    }
}