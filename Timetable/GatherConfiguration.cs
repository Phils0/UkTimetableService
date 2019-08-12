using System;
using System.Data.SqlTypes;

namespace Timetable
{
    public class GatherConfiguration
    {

        public GatherFilterFactory.GatherFilter Filter; 
        public int ServicesBefore { get; }
        public int ServicesAfter { get; }
        
        public GatherConfiguration(int before, int after, GatherFilterFactory.GatherFilter filter)
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