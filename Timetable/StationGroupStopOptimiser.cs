using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    /// <inheritdoc />
    /// <remarks>
    /// Thin orchestrator: delegates row-collapsing to <see cref="ServiceCandidateGrouping"/> and per-run stop
    /// selection to an injected <see cref="ICanonicalStopSelector"/>, then logs a request summary. All the
    /// priority/heuristic/override policy lives in the selector; the grouping identity decision lives in the helper.
    /// </remarks>
    public class StationGroupStopOptimiser : IStationGroupStopOptimiser
    {
        private readonly ICanonicalStopSelector _selector;
        private readonly ILogger _logger;

        public StationGroupStopOptimiser(ICanonicalStopSelector selector, ILogger logger)
        {
            _selector = selector;
            _logger = logger;
        }

        public ResolvedServiceStop[] OptimiseDepartures(
            IEnumerable<ResolvedServiceStop> candidates,
            StationGroup? originGroup,
            StationGroup? destinationGroup)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));

            var input = candidates as IReadOnlyCollection<ResolvedServiceStop> ?? candidates.ToList();
            var result = ServiceCandidateGrouping.PickOnePerService(
                input, sameService => _selector.ChooseDeparture(sameService, originGroup, destinationGroup));

            LogOptimisation("departures", input.Count, result.Length, originGroup, destinationGroup);
            return result;
        }

        public ResolvedServiceStop[] OptimiseArrivals(
            IEnumerable<ResolvedServiceStop> candidates,
            StationGroup? originGroup,
            StationGroup? destinationGroup)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));

            var input = candidates as IReadOnlyCollection<ResolvedServiceStop> ?? candidates.ToList();
            var result = ServiceCandidateGrouping.PickOnePerService(
                input, sameService => _selector.ChooseArrival(sameService, originGroup, destinationGroup));

            LogOptimisation("arrivals", input.Count, result.Length, originGroup, destinationGroup);
            return result;
        }

        private void LogOptimisation(
            string direction, int candidateCount, int serviceCount,
            StationGroup? originGroup, StationGroup? destinationGroup)
        {
            _logger.Debug(
                "Station-group {Direction}: {Candidates} candidates collapsed to {Services} services " +
                "(origin {OriginGroup}, destination {DestinationGroup})",
                direction, candidateCount, serviceCount,
                originGroup?.Code ?? "(none)", destinationGroup?.Code ?? "(none)");
        }
    }
}