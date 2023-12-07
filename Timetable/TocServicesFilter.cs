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
        
        public (LookupStatus status, ResolvedService[] services) GetServicesByToc(string toc, DateTime date, Time dayBoundary, bool returnCancelled)
        {
            var filtered = GetFilteredTocServices(toc, date, dayBoundary, returnCancelled);
            return (reason: Status(filtered), filtered);;
        }

        private static LookupStatus Status(ResolvedService[] filtered)
        {
            return filtered.Any() ? LookupStatus.Success : LookupStatus.ServiceNotFound;
        }

        private ResolvedService[] GetFilteredTocServices(string toc, DateTime date, Time dayBoundary, bool returnCancelled)
        {
            var services = _timetable.GetSchedulesByToc(toc, date, dayBoundary);
            var filtered = _filters.Filter(services.services, returnCancelled);
            return filtered;
        }

        public (LookupStatus status, ResolvedService[] services) GetServicesByTrainIdentity(string toc, DateTime date, string TrainIdentity, Time dayBoundary, bool returnCancelled)
        {
            var services = GetFilteredTocServices(toc, date, dayBoundary, returnCancelled);
            var filtered = services.Where(s => s.Details.TrainIdentity == TrainIdentity).ToArray();
            return (reason: Status(filtered), filtered);;
        }
    }
}