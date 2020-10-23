using System.Threading;
using System.Threading.Tasks;

namespace Timetable.DataLoader
{
    /// <summary>
    /// Timetable data loader
    /// </summary>
    public interface ICifLoader
    {
        /// <summary>
        /// Load Station Master List
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Returns list of locations loaded from the master station list</returns>
        Task<ILocationData> LoadStationMasterListAsync(CancellationToken token);

        /// <summary>
        /// Load CIf file
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="tocs"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Data> LoadCif(ILocationData locations, TocLookup tocs, CancellationToken token);       
    }
}