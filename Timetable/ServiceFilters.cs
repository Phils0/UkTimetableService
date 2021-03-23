using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    internal interface IServiceFilter
    {
        IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services);
        IEnumerable<ResolvedServiceStop> Filter(IEnumerable<ResolvedServiceStop> services);
    }

    public class ServiceFilters
    {
        private readonly bool _enableDebugResponses;
        private readonly ILogger _logger;
        private readonly ServiceAssociationBrokenFilter _brokenFilter = new ServiceAssociationBrokenFilter();
        
        public ServiceFilters(bool enableDebugResponses, ILogger logger)
        {
            _enableDebugResponses = enableDebugResponses;
            _logger = logger;
        }
        
        internal ResolvedService[] RemoveBrokenServices(IEnumerable<ResolvedService> services)
        {
            return (_enableDebugResponses ? services : _brokenFilter.Filter(services)).ToArray();
        }
        
        internal ResolvedService[] Deduplicate(IEnumerable<ResolvedService> services)
        {
            return Filter(services, true);
        }
        
        private static IServiceFilter CreateFilter(bool returnCancelled)
        {
            return returnCancelled ? (IServiceFilter) new ServiceDeduplicator() : new ServiceCancelledFilter();
        }
        
        internal ResolvedService[] Filter(IEnumerable<ResolvedService> services, bool returnCancelled)
        {
            var filter = CreateFilter(returnCancelled);
            return RemoveBrokenServices(filter.Filter(services)).ToArray();
        }
        
        public (FindStatus status, ResolvedServiceStop[] services) Filter(IEnumerable<ResolvedServiceStop> services, bool returnCancelled)
        {
            var filter = CreateFilter(returnCancelled);
            var filtered = filter.Filter(services);
            var returned = (_enableDebugResponses ? filtered : _brokenFilter.Filter(filtered)).ToArray();
            var reason = returned.Any() ? FindStatus.Success : FindStatus.NoServicesForLocation;
            return (reason, returned);
        }
    }
}