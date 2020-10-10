using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Timetable
{
    /// <summary>
    /// Timetable data loader
    /// </summary>
    public interface IDataLoader
    {        /// <summary>
        /// Load Knowledgebase Tocs
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Returns list of tocs loaded from the knowledgebase toc data</returns>
        Task<TocLookup> LoadKnowledgebaseTocsAsync(CancellationToken token);

        /// <summary>
        /// Update Station with Knowledgebase Station data
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="token"></param>
        /// <returns>Returns list of tocs loaded from the knowledgebase toc data</returns>
        Task<IEnumerable<Location>> UpdateLocationsWithKnowledgebaseStationsAsync(IEnumerable<Location> locations, CancellationToken token);
        
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
        /// <returns></returns>
        Task<Data> LoadAsync(CancellationToken cancellationToken);       
    }
}
