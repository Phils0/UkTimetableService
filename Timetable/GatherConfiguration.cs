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
            : this(new ResultWindow(before, after), all, filter)
        {
        }

        public GatherConfiguration(ResultWindow window, bool all, GatherFilter filter)
        {
            Filter = filter;
            ServicesBefore = window.Before;
            ServicesAfter = window.After;
            All = all;
        }
        
        public bool HasRequestedBeforeOnly => ServicesAfter == 0;
    }
}