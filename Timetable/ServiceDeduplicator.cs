using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    internal class ServiceDeduplicator : IServiceFilter
    {
        public IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services)
        {
            var dedupedServices = services
                .GroupBy(s => $"{s.Details.Properties.RetailServiceId}|{s.Details.Properties.TrainIdentity}")
                .SelectMany(g => Dedup<ResolvedService>(g, (s) => s.IsCancelled))
                .ToArray();
            return dedupedServices;
        }
        
        private IEnumerable<T> Dedup<T>(IGrouping<string, T> services, Func<T, bool> IsCancelled)
        {
            if (services.Any(s => IsCancelled(s)) && services.Any(s => !IsCancelled(s)))
            {
                return services.Where(s => !IsCancelled(s));
            }
            return services;
        }
        
        public IEnumerable<ResolvedServiceStop> Filter(IEnumerable<ResolvedServiceStop> services)
        {
            var dedupedServices = services
                .GroupBy(s => $"{s.Service.Details.Properties.RetailServiceId}|{s.Service.Details.Properties.TrainIdentity}")
                .SelectMany(g => Dedup<ResolvedServiceStop>(g, (s) => s.Service.IsCancelled))
                .ToArray();
            return dedupedServices;
        }
    }
}