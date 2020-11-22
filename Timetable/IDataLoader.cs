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
        /// Load timetable data
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Data> LoadAsync(CancellationToken cancellationToken);       
    }
}
