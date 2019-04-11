using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Timetable
{
    /// <summary>
    /// Timetable data loader
    /// </summary>
    public interface IDataLoader
    {
        /// <summary>
        /// Load Station Master List
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Returns list of locations loaded from the master station list</returns>
        Task<IEnumerable<Location>> LoadStationMasterListAsync(CancellationToken token);
        
        /// <summary>
        /// Load CIf file data
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="locations">Timetable Data structure with station master list locations already loaded</param>
        /// <returns></returns>
        Task<ILocationData> LoadAsync(CancellationToken cancellationToken);       
    }
}
