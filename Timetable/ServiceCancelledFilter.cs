using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    internal class ServiceCancelledFilter : IServiceFilter
    {
        public IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services)
        {
            var filteredServices = services
                .Where(s => !s.IsCancelled)
                .ToArray();
            return filteredServices;
        }
        
        public IEnumerable<ResolvedServiceStop> Filter(IEnumerable<ResolvedServiceStop> services)
        {
            var filteredServices = services
                .Where(s => !s.Service.IsCancelled)
                .ToArray();
            return filteredServices;
        }
    }
}