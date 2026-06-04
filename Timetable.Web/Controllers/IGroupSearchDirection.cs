namespace Timetable.Web.Controllers
{
    /// <summary>
    /// The direction-specific policy a <see cref="GroupSearchOrchestrator"/> needs: which optimiser entry-point to
    /// call (and how the path-side and query-side groups map onto its origin/destination), and which stop time
    /// orders/windows the results. Departures key off the origin departure; arrivals off the destination arrival.
    /// Implemented by the controllers themselves (one per endpoint) so the orchestrator stays direction-neutral and
    /// can be a single stateless singleton - the direction is supplied per call.
    /// </summary>
    public interface IGroupSearchDirection
    {
        ResolvedServiceStop[] Optimise(ResolvedServiceStop[] candidates, StationGroup? pathGroup, StationGroup? queryGroup);

        Time TimeAtFoundStop(ResolvedServiceStop stop);
    }
}
