using System.Threading;
using System.Threading.Tasks;

namespace Timetable.DataLoader
{
    /// <summary>
    /// Darwin Timetable data loader
    /// </summary>
    public interface IDarwinLoader
    {
        /// <summary>
        /// Update Station with Darwin Station data, includes via 
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="lookup"></param>
        /// <param name="token"></param>
        /// <returns>Updated Location data</returns>
        Task<ILocationData> UpdateLocationsAsync(ILocationData locations, TocLookup lookup, CancellationToken token);
        
        /// <summary>
        /// Update Tocs with Darwin Tocs data
        /// </summary>
        /// <param name="tocs"></param>
        /// <param name="token"></param>
        /// <returns>Updated toc data </returns>
        Task<TocLookup> UpdateTocsAsync(TocLookup tocs, CancellationToken token);
        
        /// <summary>
        /// Add Cancellation and Late running reasons
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Data with reasons code</returns>
        Task<Data> AddReasonsAsync(Data data, CancellationToken cancellationToken);

        /// <summary>
        /// Adds the Darwin timetable
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Data with reasons code</returns>
        Task<Data> AddDarwinTimetableAsync(Data data, CancellationToken cancellationToken);  
    }
}