using System;
using System.Data.SqlTypes;

namespace Timetable
{
    public class GatherConfiguration
    {
        public static readonly Func<ResolvedServiceStop,bool> NoFilter = (s => true);
        
        private readonly Func<ResolvedServiceStop, bool> _filter;
        public int ServicesBefore { get; }
        public int ServicesAfter { get; }
        
        public GatherConfiguration(int before, int after, Func<ResolvedServiceStop, bool> filter = null)
        {
            _filter = filter ?? NoFilter;
            
            // Ensure return at least one service
            if (before == 0 && after == 0)
                after = 1;
            
            ServicesBefore = before;
            ServicesAfter = after;
        }

        public bool HasRequestedBeforeOnly => ServicesAfter == 0;
    }
}