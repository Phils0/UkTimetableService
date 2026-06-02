namespace Timetable.Web.Controllers
{
    /// <summary>
    /// The direction-specific policy a <see cref="GroupSearchOrchestrator"/> needs: which optimiser entry-point to
    /// call (and how the path-side and query-side groups map onto its origin/destination), and which stop time
    /// orders/windows the results. Departures key off the origin departure; arrivals off the destination arrival.
    /// Two implementations, one per endpoint, so the orchestrator stays direction-neutral.
    /// </summary>
    public interface IGroupSearchDirection
    {
        ResolvedServiceStop[] Optimise(ResolvedServiceStop[] candidates, StationGroup? pathGroup, StationGroup? queryGroup);

        Time TimeAtFoundStop(ResolvedServiceStop stop);
    }

    public sealed class DeparturesDirection : IGroupSearchDirection
    {
        private readonly IStationGroupStopOptimiser _optimiser;

        public DeparturesDirection(IStationGroupStopOptimiser optimiser) => _optimiser = optimiser;

        // For /departures the path parameter is the origin and ?to= is the destination.
        public ResolvedServiceStop[] Optimise(ResolvedServiceStop[] candidates, StationGroup? pathGroup, StationGroup? queryGroup) =>
            _optimiser.OptimiseDepartures(candidates, originGroup: pathGroup, destinationGroup: queryGroup);

        public Time TimeAtFoundStop(ResolvedServiceStop stop) => ((IDeparture)stop.Stop.Stop).Time;
    }

    public sealed class ArrivalsDirection : IGroupSearchDirection
    {
        private readonly IStationGroupStopOptimiser _optimiser;

        public ArrivalsDirection(IStationGroupStopOptimiser optimiser) => _optimiser = optimiser;

        // For /arrivals the path parameter is the destination and ?from= is the origin.
        public ResolvedServiceStop[] Optimise(ResolvedServiceStop[] candidates, StationGroup? pathGroup, StationGroup? queryGroup) =>
            _optimiser.OptimiseArrivals(candidates, originGroup: queryGroup, destinationGroup: pathGroup);

        public Time TimeAtFoundStop(ResolvedServiceStop stop) => ((IArrival)stop.Stop.Stop).Time;
    }
}