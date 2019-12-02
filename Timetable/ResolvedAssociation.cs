using System;

namespace Timetable
{
    public class ResolvedAssociation
    {
        public bool IsCancelled { get; }
        public ResolvedService AssociatedService { get; }
        public DateTime On { get; }
        public Association Details { get; }

        public ResolvedAssociation(Association association, DateTime on, bool isCancelled, ResolvedService associatedService)
        {
            Details = association;
            On = on;
            IsCancelled = isCancelled;
            AssociatedService = associatedService;
        }
        public ScheduleLocation GetAssociationStop(string timetableId)
        {
            var service = Details.GetService(timetableId);
            throw new NotImplementedException();
        }
        
        public override string ToString()
        {
            return IsCancelled ? $"{Details} {On.ToYMD()} {AssociatedService} CANCELLED" : $"{Details} {On.ToYMD()} {AssociatedService}";
        }
    }
}