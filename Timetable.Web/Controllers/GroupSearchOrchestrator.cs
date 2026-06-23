using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable.Web.Controllers
{
    /// <summary>
    /// Owns the station-group search mechanics that sit between the controllers (HTTP shell) and the
    /// <see cref="IStationGroupStopOptimiser"/> (collapse policy): gathering across a group's members, building the
    /// post-dedup collapse-and-re-window transform, and re-windowing a merged board back around the request time.
    /// Stateless and direction-neutral: the per-request <see cref="IGroupSearchDirection"/> (the calling controller)
    /// is passed in to <see cref="BuildOptimise"/>, so this is registered as a single shared singleton.
    /// </summary>
    public class GroupSearchOrchestrator
    {
        private static readonly Func<ResolvedServiceStop[], ResolvedServiceStop[]> NoOptimisation = stops => stops;

        private readonly ILogger _logger;

        public GroupSearchOrchestrator(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gathers results across every member of a path-side group, concatenating the per-member finds. The status is
        /// Success if any member returned services. A member with no matching services is expected (its trains just
        /// don't match this window/filter) and stays silent. Any other per-member status is logged and dropped from
        /// the merged result, and - when no member succeeded - the most severe such status is returned rather than
        /// masking it as NoServicesForLocation, so an Error surfaces as 500 (not 404).
        /// </summary>
        public (FindStatus status, ResolvedServiceStop[] services) GatherAcrossGroupMembers(
            StationGroup group, Func<string, (FindStatus status, ResolvedServiceStop[] services)> findAtMember)
        {
            var all = new List<ResolvedServiceStop>();
            var anySuccess = false;
            var failure = FindStatus.NoServicesForLocation;
            foreach (var member in group.Members)
            {
                var (status, services) = findAtMember(member.ThreeLetterCode);
                switch (status)
                {
                    case FindStatus.Success:
                        all.AddRange(services);
                        anySuccess = true;
                        break;
                    case FindStatus.NoServicesForLocation:
                        break; // expected: this member simply has no matching services in the window/filter
                    default:
                        _logger.Warning(
                            "Station group {Group} member {Member} returned {Status}; dropping it from the merged result",
                            group.Code, member.ThreeLetterCode, status);
                        failure = MoreSevereStatus(failure, status);
                        break;
                }
            }

            return anySuccess
                ? (FindStatus.Success, all.ToArray())
                : (failure, Array.Empty<ResolvedServiceStop>());
        }

        // Failure precedence when no member returned services: Error (500) beats LocationNotFound (404) beats the
        // default NoServicesForLocation (404), so a genuine server error isn't hidden behind a "no services" 404.
        private static FindStatus MoreSevereStatus(FindStatus current, FindStatus candidate)
        {
            if (current == FindStatus.Error || candidate == FindStatus.Error)
                return FindStatus.Error;

            if (current == FindStatus.LocationNotFound || candidate == FindStatus.LocationNotFound)
                return FindStatus.LocationNotFound;

            return FindStatus.NoServicesForLocation;
        }

        /// <summary>
        /// Returns the post-dedup transform the controller hands to its Process pipeline: collapse a service's
        /// member-stops to one canonical row via the direction's optimiser. When the path side was a group the
        /// per-member boards were gathered and concatenated, so the merged set is re-ordered by time: windowed
        /// (<paramref name="pivot"/> non-null) re-windows around the request time, full-day sorts chronologically.
        /// When the path side is a plain CRS (single, already-ordered board) the order is left as-is, so the
        /// plain-CRS path is byte-for-byte unchanged.
        /// </summary>
        public Func<ResolvedServiceStop[], ResolvedServiceStop[]> BuildOptimise(
            IGroupSearchDirection direction, StationGroup? pathGroup, StationGroup? queryGroup, DateTime? pivot, ResultWindow window)
        {
            if (pathGroup == null && queryGroup == null)
                return NoOptimisation;

            return stops =>
            {
                var optimised = direction.Optimise(stops, pathGroup, queryGroup);
                if (pathGroup == null)
                    return optimised;

                return pivot != null
                    ? ReWindow(direction, optimised, pivot.Value, window)
                    : OrderChronologically(direction, optimised);
            };
        }

        /// <summary>
        /// Trims a merged board back to the windowed contract: up to <paramref name="window"/>'s before/after counts
        /// either side of the pivot. Orders and partitions by the absolute instant (running date + stop time) so a
        /// window that crosses midnight - where a stop's time-of-day alone is ambiguous - stays correct regardless of
        /// whether a next-day stop is held as 24:10 or as next-day 00:10.
        /// </summary>
        private static ResolvedServiceStop[] ReWindow(
            IGroupSearchDirection direction, IEnumerable<ResolvedServiceStop> stops, DateTime pivot, ResultWindow window)
        {
            var ordered = OrderChronologically(direction, stops);
            var beforePivot = ordered.Where(s => DateTimeAtFoundStop(direction, s) < pivot).TakeLast(window.Before);
            var fromPivot = ordered.Where(s => DateTimeAtFoundStop(direction, s) >= pivot).Take(window.After);
            return beforePivot.Concat(fromPivot).ToArray();
        }

        // Orders a merged board chronologically by the absolute instant (running date + stop time), so next-day
        // stops held as 24:10 sort after the same evening's 23:50 rather than ahead of it as a bare 00:10 would.
        private static ResolvedServiceStop[] OrderChronologically(
            IGroupSearchDirection direction, IEnumerable<ResolvedServiceStop> stops) =>
            stops.OrderBy(s => DateTimeAtFoundStop(direction, s)).ToArray();

        private static DateTime DateTimeAtFoundStop(IGroupSearchDirection direction, ResolvedServiceStop stop) =>
            stop.Stop.On.Add(direction.TimeAtFoundStop(stop).Value);
    }
}