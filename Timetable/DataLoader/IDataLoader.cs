using System.Threading;
using System.Threading.Tasks;

namespace Timetable.DataLoader
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
        Task<ILocationData> LoadStationMasterListAsync(CancellationToken token);
        
        /// <summary>
        /// Load CIf file data
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Data> LoadAsync(CancellationToken cancellationToken);       
    }
}
