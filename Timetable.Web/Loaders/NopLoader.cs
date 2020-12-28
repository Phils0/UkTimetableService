using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Timetable.Web.Loaders
{
    public class NopLoader : IDataEnricher
    {
        public Task<Data> EnrichReferenceDataAsync(Data data, CancellationToken token)
        {
            data.Darwin = new RealtimeData()
            {
                CancelReasons = new Dictionary<int, string>(),
                LateRunningReasons = new Dictionary<int, string>(),
                Sources = new Dictionary<string, string>()
            };            
            return Task.FromResult(data);
        }

        public Task<Data> EnrichTimetableAsync(Data data, CancellationToken cancellationToken)
        {
            return Task.FromResult(data);
        }
    }
}