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

    internal class ServiceDeduplicator : IServiceFilter
    {
        public IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services)
        {
            var dedupedServices = services
                .GroupBy(s => $"{s.Details.RetailServiceId}|{s.Details.TrainIdentity}")
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
                .GroupBy(s => $"{s.Service.Details.RetailServiceId}|{s.Service.Details.TrainIdentity}")
                .SelectMany(g => Dedup<ResolvedServiceStop>(g, (s) => s.Service.IsCancelled))
                .ToArray();
            return dedupedServices;
        }
    }
}