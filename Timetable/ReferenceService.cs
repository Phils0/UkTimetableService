using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Timetable
{
    public interface IReference
    {
        Task<IEnumerable<Station>> GetLocationsAsync(CancellationToken cancellationToken);
    }

    public class ReferenceService : IReference
    {
        private readonly Data _data;

        public ReferenceService(Data data)
        {
            _data = data;
        }

        public async Task<IEnumerable<Station>> GetLocationsAsync(CancellationToken cancellationToken)
        {
            return await Task.FromResult(_data.Locations.Select(p => p.Value)).ConfigureAwait(false);
        }
    }
}
