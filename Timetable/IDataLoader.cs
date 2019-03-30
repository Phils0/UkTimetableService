using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Timetable
{
    public interface IDataLoader
    {
        Task<IEnumerable<Location>> GetStationMasterListAsync(CancellationToken cancellationToken);
    }
}
