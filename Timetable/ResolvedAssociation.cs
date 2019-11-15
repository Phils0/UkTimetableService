using System;

namespace Timetable
{
    public class ResolvedAssociation
    {
        public bool IsCancelled { get; }
        public ResolvedService AssociatedService { get; }
        public DateTime On { get; }
        public Association Details { get; }
        public ResolvedServiceWithAssociations Resolved { get; private set; }

        public ResolvedAssociation(Association association, DateTime on, bool isCancelled, ResolvedService associatedService)
        {
            Details = association;
            On = on;
            IsCancelled = isCancelled;
            AssociatedService = associatedService;
        }
        
        public override string ToString()
        {
            return IsCancelled ? $"{Details} {On.ToYMD()} {AssociatedService} CANCELLED" : $"{Details} {On.ToYMD()} {AssociatedService}";
        }
    }
}