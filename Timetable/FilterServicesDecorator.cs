using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    internal interface IServiceFilter
    {
        IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services);
        IEnumerable<ResolvedServiceStop> Filter(IEnumerable<ResolvedServiceStop> services);
    }
    
    public class FilterServicesDecorator
    {
        private readonly ITimetableLookup _timetable;

        public FilterServicesDecorator(ITimetableLookup timetable)
        {
            _timetable = timetable;
        }
        
        internal static ResolvedService[] Filter(IEnumerable<ResolvedService> services, bool returnCancelled)
        {
            var filter = returnCancelled ? (IServiceFilter) new ServiceDeduplicator() : new ServiceCancelledFilter();
            return filter.Filter(services).ToArray();
        }
        
        public ITimetableLookup.GetServicesByToc GetServicesByToc(bool returnCancelled)
        {
            return (string toc, DateTime date, Time dayBoundary) =>
            {
                var services = _timetable.GetSchedulesByToc(toc, date, dayBoundary);
                var returnedServices = Filter(services.services, returnCancelled);
                var reason = returnedServices.Any() ? LookupStatus.Success : LookupStatus.ServiceNotFound;
                return (reason, returnedServices);;
            };
        }
    }
}