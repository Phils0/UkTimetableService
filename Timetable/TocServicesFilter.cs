using System;
using System.Linq;

namespace Timetable
{
    public class TocServicesFilter
    {
        private readonly ITimetableLookup _timetable;
        private readonly ServiceFilters _filters;

        public TocServicesFilter(ITimetableLookup timetable, ServiceFilters filters)
        {
            _timetable = timetable;
            _filters = filters;
        }
        
        public ITimetableLookup.GetServicesByToc GetServicesByToc(bool returnCancelled)
        {
            return (string toc, DateTime date, Time dayBoundary) =>
            {
                var services = _timetable.GetSchedulesByToc(toc, date, dayBoundary);
                var filtered = _filters.Filter(services.services, returnCancelled);
                var reason = filtered.Any() ? LookupStatus.Success : LookupStatus.ServiceNotFound;
                return (reason, filtered);;
            };
        }
    }
}