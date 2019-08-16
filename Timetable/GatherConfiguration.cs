using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace Timetable
{
    public class GatherConfiguration
    {
        public delegate IEnumerable<ResolvedServiceStop> GatherFilter(IEnumerable<ResolvedServiceStop> source);
        
        public GatherFilter Filter; 
        public int ServicesBefore { get; }
        public int ServicesAfter { get; }
        
        public GatherConfiguration(ushort before, ushort after, GatherFilter filter)
        {
            Filter = filter;
            
            // Ensure return at least one service
            if (before == 0 && after == 0)
                after = 1;
            
            ServicesBefore = before;
            ServicesAfter = after;
        }
        
        public bool HasRequestedBeforeOnly => ServicesAfter == 0;
    }
}