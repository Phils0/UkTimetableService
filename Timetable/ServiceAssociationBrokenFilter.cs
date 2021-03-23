using System;
using System.Collections.Generic;

namespace Timetable
{
    internal class ServiceAssociationBrokenFilter : IServiceFilter
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
            foreach (var service in services)
            {
                RemoveBrokenAssociations(service, getService);
            }
            return services;
        }

        private T RemoveBrokenAssociations<T>(T original, Func<T, ResolvedService> getService)
        {
            var service = getService(original);
            if (service.HasAssociations())
            {
                var withAssociation = (ResolvedServiceWithAssociations) service;
                withAssociation.RemoveBrokenAssociations();
            }
            return original;
        }
    }
}