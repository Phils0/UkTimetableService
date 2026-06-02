using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// Resolves a station-group search to one canonical <see cref="ResolvedServiceStop"/> per service.
    /// When the path parameter was a group, it <em>selects</em> the candidate whose
    /// <see cref="ResolvedServiceStop.Stop"/> is the canonical member. When the query parameter was a group,
    /// it <em>overrides</em> <see cref="ResolvedServiceStop.FoundToStop"/> (/departures) or
    /// <see cref="ResolvedServiceStop.FoundFromStop"/> (/arrivals). Both decisions apply the group's priority
    /// list first, then fall back to the configured <see cref="JourneyHeuristic"/>.
    /// </summary>
    /// <example>
    /// For a /departures search from EUS to GB@MA where the group has priorities=["MAN"]:
    /// a service stopping MCO &#8594; MCV &#8594; MAN appears once in the result, with FoundToStop set to MAN
    /// (the priority), regardless of which Manchester station the filter initially chose.
    /// </example>
    public interface IStationGroupStopOptimiser
    {
        /// <summary>
        /// Returns one canonical <see cref="ResolvedServiceStop"/> per (TimetableUid, running date)
        /// for the /departures endpoint. Either group may be <c>null</c> when that side was a single CRS.
        /// </summary>
        ResolvedServiceStop[] OptimiseDepartures(
            IEnumerable<ResolvedServiceStop> candidates,
            StationGroup? originGroup,
            StationGroup? destinationGroup);

        /// <summary>
        /// Returns one canonical <see cref="ResolvedServiceStop"/> per (TimetableUid, running date)
        /// for the /arrivals endpoint. Either group may be <c>null</c> when that side was a single CRS.
        /// </summary>
        ResolvedServiceStop[] OptimiseArrivals(
            IEnumerable<ResolvedServiceStop> candidates,
            StationGroup? originGroup,
            StationGroup? destinationGroup);
    }
}