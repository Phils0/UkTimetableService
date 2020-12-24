using System.Threading;
using System.Threading.Tasks;

namespace Timetable.Web.Loaders
{
    public class NopLoader : IDataEnricher
    {
        public Task<Data> EnrichReferenceDataAsync(Data data, CancellationToken token)
        {
            return Task.FromResult(data);
        }

        public Task<Data> EnrichTimetableAsync(Data data, CancellationToken cancellationToken)
        {
            return Task.FromResult(data);
        }
    }
}