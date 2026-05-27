using System.Diagnostics.CodeAnalysis;

namespace Timetable
{
    /// <summary>
    /// Maintains an in-memory IReadOnlyDictionary of known <see cref="StationGroup"/>s.
    /// Provides a lookup method to retrieve a single <see cref="StationGroup"/> by code.
    /// </summary>
    public interface IStationGroupMapper
    {
        /// <summary>
        /// Maps a station group code to its <see cref="StationGroup"/>.
        /// Lookup is case-insensitive and unknown codes return false.
        /// </summary>
        /// <param name="code">Station group code (e.g. <c>GB@LO</c>)</param>
        /// <param name="group">The resulting <see cref="StationGroup"/> from the dictionary</param>
        /// <returns></returns>
        bool TryGet(string code, [NotNullWhen(true)] out StationGroup? group);
    }
}