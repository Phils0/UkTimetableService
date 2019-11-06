using System;

namespace Timetable
{
    public class ResolvedAssociation
    {
        public bool IsCancelled { get; }
        
        public DateTime On { get; }

        public Association Details { get; }

        public ResolvedAssociation(Association association, DateTime on, bool isCancelled)
        {
            Details = association;
            On = on;
            IsCancelled = isCancelled;
        }
        
        public override string ToString()
        {
            return IsCancelled ? $"{Details} {On.ToYMD()} CANCELLED" : $"{Details} {On.ToYMD()}";
        }
    }
}