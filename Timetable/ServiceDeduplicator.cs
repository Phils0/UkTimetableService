using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    internal interface IServiceFilter
    {
        IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services);
    }

    internal class ServiceDeduplicator : IServiceFilter
    {
        public IEnumerable<ResolvedService> Filter(IEnumerable<ResolvedService> services)
        {
            var dedupedServices = services
                .GroupBy(s => $"{s.Details.RetailServiceId}|{s.Details.TrainIdentity}")
                .SelectMany(Dedup)
                .ToArray();
            return dedupedServices;
        }

        private IEnumerable<ResolvedService> Dedup(IGrouping<string, ResolvedService> services)
        {
            if (services.Any(s => s.IsCancelled) && services.Any(s => !s.IsCancelled))
            {
                return services.Where(s => !s.IsCancelled);
            }
            return services;
        }
    }
}