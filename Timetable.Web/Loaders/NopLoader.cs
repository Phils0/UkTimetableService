using System.Threading;
using System.Threading.Tasks;
using Timetable.DataLoader;

namespace Timetable.Web.Loaders
{
    public class NopLoader : IDarwinLoader
    {
        public Task<Data> UpdateWithDarwinReferenceAsync(Data data, CancellationToken token)
        {
            return Task.FromResult(data);
        }

        public Task<ILocationData> UpdateLocationsAsync(ILocationData locations, TocLookup lookup, CancellationToken token)
        {
            return Task.FromResult(locations);
        }

        public Task<TocLookup> UpdateTocsAsync(TocLookup tocs, CancellationToken token)
        {
            return Task.FromResult(tocs);
        }

        public Task<Data> AddReasonsAsync(Data data, CancellationToken cancellationToken)
        {
            return Task.FromResult(data);
        }

        public Task<Data> AddDarwinTimetableAsync(Data data, CancellationToken cancellationToken)
        {
            return Task.FromResult(data);
        }
    }
}