using System;
using System.Collections.Generic;

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
        
        public ScheduleLocation GetStop(ResolvedService service)
        {
            return IsMain(service.TimetableUid) ?
                Details.Main.GetStop(service) :
                Details.Associated.GetStop(service);
        }

        public bool IsMain(string timetableUid)
        {
            return Details.IsMain(timetableUid);
        }
        
        public override string ToString()
        {
            return IsCancelled ? $"{Details} {On.ToYMD()} {AssociatedService} CANCELLED" : $"{Details} {On.ToYMD()} {AssociatedService}";
        }
    }
}