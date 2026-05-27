using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Timetable
{
    /// <summary>
    /// Maintains an in-memory IReadOnlyDictionary of known <see cref="StationGroup"/>s.
    /// Provides a lookup method to retrieve a single <see cref="StationGroup"/> by code.
    /// </summary>
    public class StationGroupLookup
    {
        private readonly IReadOnlyDictionary<string, StationGroup> _groups;

        public StationGroupLookup(IEnumerable<StationGroup> groups)
        {
            if (groups == null)
                throw new ArgumentNullException(nameof(groups));

            var map = new Dictionary<string, StationGroup>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in groups)
            {
                if (map.ContainsKey(group.Code))
                    throw new ArgumentException($"Duplicate group code: {group.Code}", nameof(groups));
                map.Add(group.Code, group);
            }

            _groups = map;
        }

        /// <summary>
        /// Attempts to look up a <see cref="StationGroup"/> by its group code.
        /// Lookup is case-insensitive and unknown codes return false.
        /// </summary>
        /// <param name="code">Station group code (e.g. <c>GB@LO</c>)</param>
        /// <param name="group">The resulting <see cref="StationGroup"/> from the dictionary</param>
        /// <returns></returns>
        public bool TryGet(string code, [NotNullWhen(true)] out StationGroup? group)
        {
            if (string.IsNullOrEmpty(code))
            {
                group = null;
                return false;
            }

            return _groups.TryGetValue(code, out group);
        }
    }
}