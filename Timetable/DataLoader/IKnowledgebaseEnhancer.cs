using System.Threading;
using System.Threading.Tasks;

namespace Timetable.DataLoader
{
    /// <summary>
    /// Knowledgebase loader
    /// </summary>
    public interface IKnowledgebaseEnhancer
    {        
        /// <summary>
        /// Updates Tocs with Knowledgebase Tocs data
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Returns list of tocs loaded from the knowledgebase toc data</returns>
        Task<TocLookup> UpdateTocsWithKnowledgebaseAsync(TocLookup lookup, CancellationToken token);

        /// <summary>
        /// Update Station with Knowledgebase Station data
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="token"></param>
        /// <returns>Returns list of tocs loaded from the knowledgebase toc data</returns>
        Task<ILocationData> UpdateLocationsWithKnowledgebaseStationsAsync(ILocationData locations, TocLookup lookup, CancellationToken token);
    }
}