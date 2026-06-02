using System;
using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// A named group of stations sharing a common code (e.g. <c>GB@LO</c> for "London (all)").
    /// The optional <see cref="Priorities"/> list defines a strict preference ordering used by the
    /// station-group optimiser when compiling search results; if null, the optimiser falls back to its
    /// configured <see cref="JourneyHeuristic"/>. The list is supplied in priority order by the loader.
    /// </summary>
    public class StationGroup
    {
        public string Code { get; }
        public IReadOnlySet<Station> Members { get; }
        public IReadOnlyList<Station>? Priorities { get; }

        public StationGroup(string code, IEnumerable<Station> members, IReadOnlyList<Station>? priorities = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Group code is required", nameof(code));
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            Code = code;
            Members = new HashSet<Station>(members);

            if (Members.Count == 0)
                throw new ArgumentException($"Group {code} must have at least one member", nameof(members));

            Priorities = priorities is { Count: > 0 } ? priorities : null;
        }
    }
}