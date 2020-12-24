using System.Threading;
using System.Threading.Tasks;

namespace Timetable.Web.Loaders
{
    /// <summary>
    /// Interface to be implemented by additional data sources to enhance the cif schedule
    /// </summary>
    public interface IDataEnricher
    {
        /// <summary>
        /// Update Reference data: stations, tocs etc
        /// </summary>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns>Updated refernce data</returns>
        Task<Data> EnrichReferenceDataAsync(Data data, CancellationToken token);
        
        /// <summary>
        /// Adds the Darwin timetable
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Data timetable</returns>
        Task<Data> EnrichTimetableAsync(Data data, CancellationToken cancellationToken);  
    }
}