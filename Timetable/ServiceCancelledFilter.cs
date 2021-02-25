using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    internal class ServiceCancelledFilter : IServiceFilter
    {
        public IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services)
        {
            var dedupedServices = services
                .Where(s => !s.IsCancelled)
                .ToArray();
            return dedupedServices;
        }
        
        public IEnumerable<ResolvedServiceStop> Filter(IEnumerable<ResolvedServiceStop> services)
        {
            var dedupedServices = services
                .Where(s => !s.Service.IsCancelled)
                .ToArray();
            return dedupedServices;
        }
    }
}