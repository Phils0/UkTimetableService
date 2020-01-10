using System;
using System.Collections.Generic;

namespace Timetable
{
    public class AssociationDictionary :
        Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>
    {
        public AssociationDictionary(int capacity) : base(capacity)
        {
        }
        
        public ResolvedAssociation[] Resolve(string timetableUid, DateTime on)
        {
            var resolvedAssociations = new List<ResolvedAssociation>();
            foreach (var versions in this.Values)
            {
                var isCancelled = false;
                foreach (var association in versions.Values)
                {
                    if (association.AppliesOn(on))
                    {
                        if (association.IsCancelled())
                            isCancelled = true;
                        else
                        {
                            var other = association.GetOtherService(timetableUid);
                            var otherDate = ResolveDate(on, association.DateIndicator, association.IsMain(timetableUid));
                            var resolved = other.GetScheduleOn(otherDate, false);
                            resolvedAssociations.Add(new ResolvedAssociation(association, on, isCancelled, resolved));
                            break;
                        }
                    }
                }
            }
            
            return  resolvedAssociations.ToArray();;
        }
        
        private DateTime ResolveDate(DateTime onDate, AssociationDateIndicator indicator, bool isMain)
        {
            switch (indicator)
            {
                case AssociationDateIndicator.Standard:
                    return onDate;
                case AssociationDateIndicator.NextDay:
                    return isMain ? onDate.AddDays(1) : onDate.AddDays(-1);
                case AssociationDateIndicator.PreviousDay:
                    return isMain ? onDate.AddDays(-1) : onDate.AddDays(1);
                default:
                    throw new ArgumentException($"Unhandled DateIndicator value {indicator}", nameof(indicator));
            }
        }
    }
}