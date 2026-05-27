using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    /// <summary>
    /// Collapses gathered candidate stops to one per physical service run, identified by
    /// <c>(TimetableUid, running date)</c>. The caller supplies the delegate
    /// that chooses the surviving row for each run, so this helper knows nothing about groups or selection process.
    /// </summary>
    internal static class ServiceCandidateGrouping
    {
        public static ResolvedServiceStop[] PickOnePerService(
            IEnumerable<ResolvedServiceStop> candidates,
            Func<IReadOnlyList<ResolvedServiceStop>, ResolvedServiceStop?> chooseCanonical)
        {
            var picked = new List<ResolvedServiceStop>();
            // TimetableUid + running date is the genuine identity of one physical service run: it is never
            // empty (unlike RetailServiceId) and stays distinct across split portions that share an RSID.
            foreach (var sameService in candidates.GroupBy(c => (c.Service.TimetableUid, c.Service.On)))
            {
                var chosen = chooseCanonical(sameService.ToList());
                if (chosen != null) picked.Add(chosen);
            }
            return picked.ToArray();
        }
    }
}