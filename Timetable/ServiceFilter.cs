using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    internal interface IServiceFilter
    {
        IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services);
        IEnumerable<ResolvedServiceStop> Filter(IEnumerable<ResolvedServiceStop> services);
    }

    public class ServiceFilter
    {
        internal static IServiceFilter CreateFilter(bool returnCancelled)
        {
            return returnCancelled ? (IServiceFilter) new ServiceDeduplicator() : new ServiceCancelledFilter();
        }
        
        internal static ResolvedService[] Filter(IEnumerable<ResolvedService> services, bool returnCancelled)
        {
            var filter = CreateFilter(returnCancelled);
            return filter.Filter(services).ToArray();
        }
        
        public (FindStatus status, ResolvedServiceStop[] services) Filter(IEnumerable<ResolvedServiceStop> services, bool returnCancelled)
        {
            var filter = ServiceFilter.CreateFilter(returnCancelled);
            var filtered = filter.Filter(services).ToArray();
            var reason = filtered.Any() ? FindStatus.Success : FindStatus.NoServicesForLocation;
            return (reason, filtered);
        }
    }
}