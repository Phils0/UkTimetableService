using System.Threading;
using System.Threading.Tasks;

namespace Timetable.Web.Loaders
{
    /// <summary>
    /// Timetable data loader
    /// </summary>
    public interface ICifLoader
    {
        /// <summary>
        /// Cif file loading
        /// </summary>
        /// <returns>Returns the file name of the cif file</returns>
        string ArchiveFile { get; }
        
        /// <summary>
        /// Load Station Master List
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Returns list of locations loaded from the master station list</returns>
        Task<ILocationData> LoadStationMasterListAsync(CancellationToken token);

        /// <summary>
        /// Load CIf file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Data> LoadCif(Data data, CancellationToken token);       
    }
}