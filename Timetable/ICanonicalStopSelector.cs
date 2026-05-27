using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// Chooses the single canonical <see cref="ResolvedServiceStop"/> for one service's candidate stops in a
    /// station-group search. Applies the group's priority list first, then the configured
    /// <see cref="JourneyHeuristic"/>; when the query parameter was a group it also overrides the found
    /// destination (/departures) or origin (/arrivals) stop.
    /// </summary>
    public interface ICanonicalStopSelector
    {
        /// <summary>
        /// Canonical stop for a /departures result. <paramref name="sameService"/> are the gathered candidates
        /// for a single service run. Either group may be <c>null</c> when that side of the journey was a single CRS.
        /// </summary>
        ResolvedServiceStop? ChooseDeparture(
            IReadOnlyList<ResolvedServiceStop> sameService,
            StationGroup? originGroup,
            StationGroup? destinationGroup);

        /// <summary>
        /// Canonical stop for an /arrivals result &mdash; the mirror of <see cref="ChooseDeparture"/>.
        /// </summary>
        ResolvedServiceStop? ChooseArrival(
            IReadOnlyList<ResolvedServiceStop> sameService,
            StationGroup? originGroup,
            StationGroup? destinationGroup);
    }
}