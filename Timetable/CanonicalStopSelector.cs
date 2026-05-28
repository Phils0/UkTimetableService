using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    /// <inheritdoc />
    /// <remarks>
    /// For a single service's gathered candidate rows:
    /// <list type="bullet">
    ///   <item><description><c>Choose*</c> select the surviving candidate when the path parameter was a group
    ///   code, collapsing the service's multiple member-stops into one.</description></item>
    ///
    ///   <item><description><c>Apply*Override</c> update <see cref="ResolvedServiceStop.FoundToStop"/> or
    ///   <see cref="ResolvedServiceStop.FoundFromStop"/> when the query parameter was a group code, replacing
    ///   the filter's natural pick (longest journey) when a priority entry or the Shortest heuristic dictates
    ///   otherwise.</description></item>
    /// </list>
    /// Both apply the group's priority list first, then fall back to the configured <see cref="JourneyHeuristic"/>.
    /// </remarks>
    public class CanonicalStopSelector : ICanonicalStopSelector
    {
        private readonly JourneyHeuristic _heuristic;

        public CanonicalStopSelector(JourneyHeuristic heuristic)
        {
            _heuristic = heuristic;
        }

        public ResolvedServiceStop? ChooseDeparture(
            IReadOnlyList<ResolvedServiceStop> sameService,
            StationGroup? originGroup,
            StationGroup? destinationGroup)
        {
            var canonical = ChooseDeparturesOrigin(sameService, originGroup);
            if (canonical != null && destinationGroup != null)
                ApplyDeparturesDestinationOverride(canonical, destinationGroup);
            return canonical;
        }

        public ResolvedServiceStop? ChooseArrival(
            IReadOnlyList<ResolvedServiceStop> sameService,
            StationGroup? originGroup,
            StationGroup? destinationGroup)
        {
            var canonical = ChooseArrivalsDestination(sameService, destinationGroup);
            if (canonical != null && originGroup != null)
                ApplyArrivalsOriginOverride(canonical, originGroup);
            return canonical;
        }

        private ResolvedServiceStop? ChooseDeparturesOrigin(
            IReadOnlyList<ResolvedServiceStop> candidates,
            StationGroup? originGroup)
        {
            if (candidates.Count <= 1 || originGroup == null) return candidates.FirstOrDefault();
            return ChooseByPriority(candidates, originGroup.Priorities)
                ?? ChooseDeparturesOriginByHeuristic(candidates);
        }

        private ResolvedServiceStop ChooseDeparturesOriginByHeuristic(IReadOnlyList<ResolvedServiceStop> candidates) =>
            _heuristic == JourneyHeuristic.Longest
                ? candidates.OrderBy(DepartureTimeAtStop, Time.EarlierLaterComparer).First()  // earliest departure -> longest journey within the group
                : candidates.OrderBy(DepartureTimeAtStop, Time.LaterEarlierComparer).First();

        private void ApplyDeparturesDestinationOverride(ResolvedServiceStop candidate, StationGroup destinationGroup)
        {
            var stop = FindDeparturesDestinationByPriority(candidate, destinationGroup.Priorities)
                       ?? FindDeparturesDestinationByHeuristic(candidate, destinationGroup);
            if (stop != null)
                candidate.OverrideFoundToStop(stop);
        }

        // Priorities are guaranteed in-group by the StationGroup constructor, so a matched stop is always a
        // member; no membership re-check is needed here.
        private static ResolvedStop? FindDeparturesDestinationByPriority(
            ResolvedServiceStop candidate,
            IReadOnlyList<Station>? priorities)
        {
            if (priorities is not { Count: > 0 }) return null;
            var departureTime = DepartureTimeAtStop(candidate);
            foreach (var priority in priorities)
            {
                foreach (var arrival in candidate.Service.Details.Arrivals)
                {
                    if (arrival.Time.IsBefore(departureTime)) continue;
                    if (priority.Equals(arrival.Station))
                        return ToResolvedStop(candidate, arrival);
                }
            }
            return null;
        }

        private ResolvedStop? FindDeparturesDestinationByHeuristic(
            ResolvedServiceStop candidate,
            StationGroup destinationGroup)
        {
            // Longest needs no override — the filter's backward scan over arrivals already picked the latest in-group stop.
            if (_heuristic == JourneyHeuristic.Longest) return null;

            // Shortest: pick the earliest in-group arrival after the candidate's origin departure.
            var departureTime = DepartureTimeAtStop(candidate);
            foreach (var arrival in candidate.Service.Details.Arrivals)
            {
                if (arrival.Time.IsBefore(departureTime)) continue;
                if (destinationGroup.Members.Contains(arrival.Station))
                    return ToResolvedStop(candidate, arrival);
            }
            return null;
        }

        private ResolvedServiceStop? ChooseArrivalsDestination(
            IReadOnlyList<ResolvedServiceStop> candidates,
            StationGroup? destinationGroup)
        {
            if (candidates.Count <= 1 || destinationGroup == null) return candidates.FirstOrDefault();
            return ChooseByPriority(candidates, destinationGroup.Priorities)
                ?? ChooseArrivalsDestinationByHeuristic(candidates);
        }

        private ResolvedServiceStop ChooseArrivalsDestinationByHeuristic(IReadOnlyList<ResolvedServiceStop> candidates) =>
            _heuristic == JourneyHeuristic.Longest
                ? candidates.OrderBy(ArrivalTimeAtStop, Time.LaterEarlierComparer).First()    // latest arrival -> longest journey within the group
                : candidates.OrderBy(ArrivalTimeAtStop, Time.EarlierLaterComparer).First();

        private void ApplyArrivalsOriginOverride(ResolvedServiceStop candidate, StationGroup originGroup)
        {
            var stop = FindArrivalsOriginByPriority(candidate, originGroup.Priorities)
                       ?? FindArrivalsOriginByHeuristic(candidate, originGroup);
            if (stop != null)
                candidate.OverrideFoundFromStop(stop);
        }

        // Priorities are guaranteed in-group by the StationGroup constructor, so a matched stop is always a
        // member; no membership re-check is needed here.
        private static ResolvedStop? FindArrivalsOriginByPriority(
            ResolvedServiceStop candidate,
            IReadOnlyList<Station>? priorities)
        {
            if (priorities is not { Count: > 0 }) return null;
            var arrivalTime = ArrivalTimeAtStop(candidate);
            foreach (var priority in priorities)
            {
                foreach (var departure in candidate.Service.Details.Departures)
                {
                    if (arrivalTime.IsBefore(departure.Time)) break;
                    if (priority.Equals(departure.Station))
                        return ToResolvedStop(candidate, departure);
                }
            }
            return null;
        }

        private ResolvedStop? FindArrivalsOriginByHeuristic(
            ResolvedServiceStop candidate,
            StationGroup originGroup)
        {
            // Longest needs no override — the filter's forward scan over departures already picked the earliest in-group stop.
            if (_heuristic == JourneyHeuristic.Longest) return null;

            // Shortest: pick the latest in-group departure before the candidate's destination arrival.
            var arrivalTime = ArrivalTimeAtStop(candidate);
            foreach (var departure in candidate.Service.Details.Departures.Reverse())
            {
                if (arrivalTime.IsBefore(departure.Time)) continue;
                if (originGroup.Members.Contains(departure.Station))
                    return ToResolvedStop(candidate, departure);
            }
            return null;
        }

        private static ResolvedServiceStop? ChooseByPriority(
            IReadOnlyList<ResolvedServiceStop> candidates,
            IReadOnlyList<Station>? priorities)
        {
            if (priorities is not { Count: > 0 }) return null;
            foreach (var priority in priorities)
            {
                foreach (var candidate in candidates)
                {
                    if (priority.Equals(candidate.Stop.Stop.Station))
                        return candidate;
                }
            }
            return null;
        }

        private static Time DepartureTimeAtStop(ResolvedServiceStop c) => ((IDeparture)c.Stop.Stop).Time;
        private static Time ArrivalTimeAtStop(ResolvedServiceStop c) => ((IArrival)c.Stop.Stop).Time;

        private static ResolvedStop ToResolvedStop(ResolvedServiceStop candidate, IStop stop) =>
            new ResolvedStop((ScheduleLocation)stop, candidate.Service.On);
    }
}