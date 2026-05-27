using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Timetable
{
    /// <inheritdoc />
    public class StationGroupMapper : IStationGroupMapper
    {
        private readonly IReadOnlyDictionary<string, StationGroup> _groups;

        public StationGroupMapper(IEnumerable<StationGroup> groups)
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

        /// <inheritdoc />
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