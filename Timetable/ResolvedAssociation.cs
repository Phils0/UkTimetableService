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
        public AssociationCategory Category => Details.Category;

        public bool IsJoin => AssociationCategory.Join.Equals(Category);
        public bool IsSplit => AssociationCategory.Split.Equals(Category);

        public ResolvedAssociation(Association association, DateTime on, bool isCancelled, ResolvedService associatedService)
        {
            Details = association;
            On = on;
            IsCancelled = isCancelled;
            AssociatedService = associatedService;
        }
        
        public ResolvedServiceStop GetStop(ResolvedService service)
        {
            var association = IsMain(service.TimetableUid) ?
                Details.Main :
                Details.Associated;

            return service.GetStop(association.AtLocation, association.Sequence);
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