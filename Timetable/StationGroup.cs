using System;
using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// A named group of stations sharing a common code (e.g. <c>GB@LO</c> for "London (all)").
    /// Members are matched case-insensitively. The optional <see cref="Priorities"/> list defines
    /// a strict preference ordering used by station-group deduplication when compiling search results;
    /// if null, the optimiser falls back to its configured <see cref="JourneyHeuristic"/>.
    /// </summary>
    public class StationGroup
    {
        public string Code { get; }
        public IReadOnlySet<string> Members { get; }
        public IReadOnlyList<string>? Priorities { get; }

        public StationGroup(string code, IEnumerable<string> members, IReadOnlyList<string>? priorities = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Group code is required", nameof(code));
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            Code = code;
            Members = new HashSet<string>(members, StringComparer.OrdinalIgnoreCase);

            if (Members.Count == 0)
                throw new ArgumentException($"Group {code} must have at least one member", nameof(members));

            Priorities = priorities is { Count: > 0 } ? priorities : null;

            // Priorities must be a subset of Members. Enforcing it here means the optimiser can trust that a
            // priority CRS is always in-group, so it never needs to re-check membership when overriding stops.
            if (Priorities != null)
                foreach (var priority in Priorities)
                    if (!Members.Contains(priority))
                        throw new ArgumentException(
                            $"Group {code} priority '{priority}' is not one of its members", nameof(priorities));
        }
    }
}