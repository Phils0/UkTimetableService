using System;
using System.Data.SqlTypes;

namespace Timetable
{
    public enum TimesToUse
    {
        NotSet = -1,
        Arrivals,
        Departures
    }
    
    public class GatherConfiguration
    {

        public GatherFilterFactory.GatherFilter Filter; 
        public int ServicesBefore { get; }
        public int ServicesAfter { get; }
        
        public TimesToUse TimesToUse { get; set; }
        
        public GatherConfiguration(int before, int after, GatherFilterFactory.GatherFilter filter) :
            this(before, after, filter, TimesToUse.NotSet)
        {
        }
        
        internal GatherConfiguration(int before, int after, GatherFilterFactory.GatherFilter filter, TimesToUse arrivalsOrDepartures)
        {
            Filter = filter;
            
            // Ensure return at least one service
            if (before == 0 && after == 0)
                after = 1;
            
            ServicesBefore = before;
            ServicesAfter = after;
            TimesToUse = arrivalsOrDepartures;
        }

        public bool HasRequestedBeforeOnly => ServicesAfter == 0;
    }
}