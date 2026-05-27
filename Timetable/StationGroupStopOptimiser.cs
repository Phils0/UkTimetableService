using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    /// <inheritdoc />
    /// <remarks>
    /// Two families of internal helpers:
    /// <list type="bullet">
    ///   <item><description><c>Choose*</c> select the canonical candidate when the path parameter was a group
    ///   code, collapsing multiple results for the same service into one.</description></item>
    ///   <item><description><c>Apply*Override</c> update <see cref="ResolvedServiceStop.FoundToStop"/> or
    ///   <see cref="ResolvedServiceStop.FoundFromStop"/> on a chosen candidate when the query parameter was a
    ///   group code, replacing the filter's natural pick (longest journey) if a priority entry or the Shortest heuristic dictates
    ///   otherwise.</description></item>
    /// </list>
    /// </remarks>
    public class StationGroupStopOptimiser : IStationGroupStopOptimiser
    {
        private readonly JourneyHeuristic _heuristic;

        public StationGroupStopOptimiser(JourneyHeuristic heuristic) => _heuristic = heuristic;

        public ResolvedServiceStop[] OptimiseDepartures(
            IEnumerable<ResolvedServiceStop> candidates,
            StationGroup? originGroup,
            StationGroup? destinationGroup)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));

            return PickOnePerService(candidates, sameService =>
            {
                var canonical = ChooseDeparturesOrigin(sameService, originGroup);
                if (canonical != null && destinationGroup != null)
                    ApplyDeparturesDestinationOverride(canonical, destinationGroup);
                return canonical;
            });
        }

        public ResolvedServiceStop[] OptimiseArrivals(
            IEnumerable<ResolvedServiceStop> candidates,
            StationGroup? originGroup,
            StationGroup? destinationGroup)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));

            return PickOnePerService(candidates, sameService =>
            {
                var canonical = ChooseArrivalsDestination(sameService, destinationGroup);
                if (canonical != null && originGroup != null)
                    ApplyArrivalsOriginOverride(canonical, originGroup);
                return canonical;
            });
        }

        private static ResolvedServiceStop[] PickOnePerService(
            IEnumerable<ResolvedServiceStop> candidates,
            Func<IReadOnlyList<ResolvedServiceStop>, ResolvedServiceStop?> chooseOne)
        {
            var picked = new List<ResolvedServiceStop>();
            foreach (var sameService in candidates.GroupBy(c => (c.Service.Details.RetailServiceId, c.Service.On)))
            {
                var chosen = chooseOne(sameService.ToList());
                if (chosen != null) picked.Add(chosen);
            }
            return picked.ToArray();
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
            if (stop != null) candidate.FoundToStop = stop;
        }

        private static ResolvedStop? FindDeparturesDestinationByPriority(
            ResolvedServiceStop candidate,
            IReadOnlyList<string>? priorities)
        {
            if (priorities is not { Count: > 0 }) return null;
            var departureTime = DepartureTimeAtStop(candidate);
            foreach (var crs in priorities)
            {
                foreach (var arrival in candidate.Service.Details.Arrivals)
                {
                    if (arrival.Time.IsBefore(departureTime)) continue;
                    if (CrsMatches(arrival.Station, crs))
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
                if (destinationGroup.Members.Contains(arrival.Station.ThreeLetterCode))
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
            if (stop != null) candidate.FoundFromStop = stop;
        }

        private static ResolvedStop? FindArrivalsOriginByPriority(
            ResolvedServiceStop candidate,
            IReadOnlyList<string>? priorities)
        {
            if (priorities is not { Count: > 0 }) return null;
            var arrivalTime = ArrivalTimeAtStop(candidate);
            foreach (var crs in priorities)
            {
                foreach (var departure in candidate.Service.Details.Departures)
                {
                    if (arrivalTime.IsBefore(departure.Time)) break;
                    if (CrsMatches(departure.Station, crs))
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
                if (originGroup.Members.Contains(departure.Station.ThreeLetterCode))
                    return ToResolvedStop(candidate, departure);
            }
            return null;
        }

        private static ResolvedServiceStop? ChooseByPriority(
            IReadOnlyList<ResolvedServiceStop> candidates,
            IReadOnlyList<string>? priorities)
        {
            if (priorities is not { Count: > 0 }) return null;
            foreach (var crs in priorities)
            {
                foreach (var candidate in candidates)
                {
                    if (CrsMatches(candidate.Stop.Stop.Station, crs))
                        return candidate;
                }
            }
            return null;
        }

        private static Time DepartureTimeAtStop(ResolvedServiceStop c) => ((IDeparture)c.Stop.Stop).Time;
        private static Time ArrivalTimeAtStop(ResolvedServiceStop c) => ((IArrival)c.Stop.Stop).Time;

        private static ResolvedStop ToResolvedStop(ResolvedServiceStop candidate, IStop stop) =>
            new ResolvedStop((ScheduleLocation)stop, candidate.Service.On);

        private static bool CrsMatches(Station station, string crs) =>
            StringComparer.OrdinalIgnoreCase.Equals(station.ThreeLetterCode, crs);
    }
}