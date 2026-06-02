using System.Threading;
using System.Threading.Tasks;

namespace Timetable.Web.Loaders
{
    /// <summary>
    /// Loads the station-group reference data from disk at startup and exposes it as a
    /// <see cref="StationGroupLookup"/>. The file is optional: when it is absent the loader
    /// returns an empty lookup so that group lookups simply fail and the feature is disabled.
    /// </summary>
    /// <remarks>
    /// Member and priority CRS codes in the file are resolved to <see cref="Station"/> instances via
    /// <paramref name="locations"/>: each <see cref="StationGroup"/> holds the resolved instances. A CRS
    /// that doesn't appear in the master station data is logged and skipped; a group with no resolvable
    /// members is dropped entirely.
    /// </remarks>
    public interface IStationGroupsLoader
    {
        /// <summary>
        /// Reads and validates the station-groups file, resolving CRS codes against
        /// <paramref name="locations"/> and returning a populated <see cref="StationGroupLookup"/>.
        /// Invalid entries are skipped with a warning; a malformed file throws.
        /// </summary>
        Task<StationGroupLookup> LoadAsync(ILocationData locations, CancellationToken token);
    }
}
