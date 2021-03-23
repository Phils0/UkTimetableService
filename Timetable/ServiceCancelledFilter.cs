using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    internal class ServiceCancelledFilter : IServiceFilter
    {
        public IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services)
        {
            return Filter(services, (s) => s);;
        }
        
        public IEnumerable<ResolvedServiceStop> Filter(IEnumerable<ResolvedServiceStop> services)
        {
            return Filter(services, (s) => s.Service);
        }
        
        private IEnumerable<T> Filter<T>(IEnumerable<T> services, Func<T, ResolvedService> getService)
        {
            var filteredServices = services
                .Where(s => !getService(s).IsCancelled)
                .ToArray();

            foreach (var service in filteredServices)
            {
                RemoveCancelledAssociations(service, getService);
            }
            return filteredServices;
        }

        private T RemoveCancelledAssociations<T>(T original, Func<T, ResolvedService> getService)
        {
            var service = getService(original);
            if (service.HasAssociations())
            {
                var withAssociation = (ResolvedServiceWithAssociations) service;
                withAssociation.RemoveCancelledAssociations();
            }
            return original;
        }
    }
}