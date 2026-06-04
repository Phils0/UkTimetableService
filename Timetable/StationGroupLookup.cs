using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Timetable
{
    /// <summary>
    /// Maintains an in-memory dictionary of known <see cref="StationGroup"/>s, keyed by group code.
    /// Provides a lookup method to retrieve a single <see cref="StationGroup"/> by code.
    /// </summary>
    /// <remarks>
    /// Case-sensitivity follows the comparer the supplied dictionary was built with. The production
    /// builder (<c>StationGroupsLoader</c>) uses <see cref="StringComparer.OrdinalIgnoreCase"/>.
    /// </remarks>
    public class StationGroupLookup
    {
        /// <summary>
        /// An empty lookup: every <see cref="TryGet"/> misses. The natural "station groups disabled" state
        /// (no data file), and a safe default wherever a lookup is required but none has been built.
        /// </summary>
        public static readonly StationGroupLookup Empty = new(new Dictionary<string, StationGroup>(StringComparer.OrdinalIgnoreCase));

        private readonly IReadOnlyDictionary<string, StationGroup> _groups;

        public StationGroupLookup(IReadOnlyDictionary<string, StationGroup> groups)
        {
            _groups = groups ?? throw new ArgumentNullException(nameof(groups));
        }

        /// <summary>
        /// Attempts to look up a <see cref="StationGroup"/> by its group code. Unknown codes return false.
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