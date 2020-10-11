using System;
using System.Linq;

namespace Timetable
{
    public class TocFilter
    {
        public string Tocs { get; }

        public bool NoFilter { get; }
        
        public bool HasInvalidTocs { get; }
        
        public TocFilter(string[] tocs)
        {
            HasInvalidTocs = GuardValidation(tocs);
            Tocs = tocs == null ? "" : string.Join("|", tocs).ToUpper();
            NoFilter = string.IsNullOrEmpty(Tocs); 
        }

        private bool GuardValidation(string[] tocs)
        {
            if(tocs == null)
                return false;
            
            return tocs
                .Where(t => !string.IsNullOrEmpty(t) && t.Length != 2)
                .Any();
        }

        public bool IsValid(Toc toc) => NoFilter || Tocs.Contains(toc.Code);
    }
}