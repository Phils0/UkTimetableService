using System;
using System.Linq;

namespace Timetable
{
    public class FilterServicesDecorator
    {
        private readonly ITimetableLookup _timetable;

        public FilterServicesDecorator(ITimetableLookup timetable)
        {
            _timetable = timetable;
        }
        
        public ITimetableLookup.GetServicesByToc GetServicesByToc(bool returnCancelled)
        {
            return (string toc, DateTime date, Time dayBoundary) =>
            {
                var services = _timetable.GetSchedulesByToc(toc, date, dayBoundary);
                var filtered = ServiceFilter.Filter(services.services, returnCancelled);
                var reason = filtered.Any() ? LookupStatus.Success : LookupStatus.ServiceNotFound;
                return (reason, filtered);;
            };
        }
    }
}