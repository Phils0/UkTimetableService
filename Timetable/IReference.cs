using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Timetable
{
    public interface IReference
    {
        Task<IDictionary<string, Station>> GetLocationsAsync(CancellationToken cancellationToken);
    }
}
