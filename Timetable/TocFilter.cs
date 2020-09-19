using System;

namespace Timetable
{
    public class TocFilter
    {
        public string Tocs { get; }

        public bool NoFilter { get; }
        
        public TocFilter(string[] tocs)
        {
            
            Tocs = tocs == null ? "" : string.Join("|", tocs).ToUpper();
            NoFilter = string.IsNullOrEmpty(Tocs); 
        }

        public bool IsValid(Toc toc) => NoFilter || Tocs.Contains(toc.Code);
    }
}