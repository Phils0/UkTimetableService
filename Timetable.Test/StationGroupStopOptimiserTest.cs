using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Serilog;
using Xunit;
using static Timetable.Test.StationGroupSearchFixtures;

namespace Timetable.Test
{
    public class StationGroupStopOptimiserTest
    {
        // A selector stub that returns the first row of each run, so we can observe how the orchestrator groups,
        // delegates and collects without depending on the real selection policy (covered by CanonicalStopSelectorTest).
        private static ICanonicalStopSelector FirstOfRun()
        {
            var selector = Substitute.For<ICanonicalStopSelector>();
            selector.ChooseDeparture(Arg.Any<IReadOnlyList<ResolvedServiceStop>>(), Arg.Any<StationGroup?>(), Arg.Any<StationGroup?>())
                .Returns(ci => ((IReadOnlyList<ResolvedServiceStop>)ci[0]).FirstOrDefault());
            selector.ChooseArrival(Arg.Any<IReadOnlyList<ResolvedServiceStop>>(), Arg.Any<StationGroup?>(), Arg.Any<StationGroup?>())
                .Returns(ci => ((IReadOnlyList<ResolvedServiceStop>)ci[0]).FirstOrDefault());
            return selector;
        }

        private static StationGroupStopOptimiser Optimiser(ICanonicalStopSelector selector) =>
            new(selector, Substitute.For<ILogger>());

        [Fact]
        public void Departures_EmptyInputReturnsEmptyWithoutConsultingSelector()
        {
            var selector = FirstOfRun();

            var result = Optimiser(selector).OptimiseDepartures(Array.Empty<ResolvedServiceStop>(), null, null);

            Assert.Empty(result);
            selector.DidNotReceiveWithAnyArgs().ChooseDeparture(default!, default, default);
        }

        [Fact]
        public void Departures_ReturnsTheSelectorsPickPerServiceRun()
        {
            var service = LondonAllToManchesterPiccadilly();
            var atKgx = new ResolvedServiceStop(service, service.Details.Locations[0]);
            var atStp = new ResolvedServiceStop(service, service.Details.Locations[1]);

            var result = Optimiser(FirstOfRun()).OptimiseDepartures(new[] { atKgx, atStp }, null, null);

            Assert.Single(result);          // one physical run in, one row out
            Assert.Same(atKgx, result[0]);  // the stub's pick
        }

        [Fact]
        public void Departures_DelegatesEachRunToTheSelectorWithBothGroups()
        {
            var selector = FirstOfRun();
            var origin = new StationGroup("GB@LO", new[] { KingsCross });
            var destination = new StationGroup("GB@MA", new[] { ManchesterPiccadilly });
            var service = LondonAllToManchesterPiccadilly();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[0]);

            Optimiser(selector).OptimiseDepartures(new[] { stop }, origin, destination);

            selector.Received(1).ChooseDeparture(Arg.Any<IReadOnlyList<ResolvedServiceStop>>(), origin, destination);
        }

        [Fact]
        public void Arrivals_DelegatesToChooseArrivalNotChooseDeparture()
        {
            var selector = FirstOfRun();
            var service = LondonAllToManchesterPiccadilly();
            var stop = new ResolvedServiceStop(service, service.Details.Locations[2]); // Manchester

            var result = Optimiser(selector).OptimiseArrivals(new[] { stop }, null, null);

            Assert.Single(result);
            selector.Received(1).ChooseArrival(Arg.Any<IReadOnlyList<ResolvedServiceStop>>(), Arg.Any<StationGroup?>(), Arg.Any<StationGroup?>());
            selector.DidNotReceiveWithAnyArgs().ChooseDeparture(default!, default, default);
        }

        [Fact]
        public void Departures_NullCandidatesThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Optimiser(FirstOfRun()).OptimiseDepartures(null!, null, null));
        }
    }
}