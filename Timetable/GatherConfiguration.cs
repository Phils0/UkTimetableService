using System.Collections.Generic;

namespace Timetable
{
    public class GatherConfiguration
    {
        public delegate IEnumerable<ResolvedServiceStop> GatherFilter(IEnumerable<ResolvedServiceStop> source);
        
        public GatherFilter Filter; 
        public int ServicesBefore { get; }
        public int ServicesAfter { get; }
        public bool All { get; }
        
        public GatherConfiguration(ushort before, ushort after, bool all, GatherFilter filter)
        {
            Filter = filter;
            
            // Ensure return at least one service
            if (before == 0 && after == 0)
                after = 1;
            
            ServicesBefore = before;
            ServicesAfter = after;
            All = all;
        }
        
        public bool HasRequestedBeforeOnly => ServicesAfter == 0;
    }
}