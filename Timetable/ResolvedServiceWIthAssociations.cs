using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public class ResolvedServiceWithAssociations : ResolvedService
    {
        public ResolvedAssociation[] Associations { get; }
        
        public  ResolvedServiceWithAssociations(ResolvedService service, Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> associations)
            : this(service.Details, service.On, service.IsCancelled, associations)
        {
        }

        public ResolvedServiceWithAssociations(Schedule service, DateTime on, bool isCancelled, IDictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> associations)
            : this(service, on, isCancelled, ResolveAssociations(service.TimetableUid, on, associations))
        {
        }
        
        internal ResolvedServiceWithAssociations(ResolvedService service, ResolvedAssociation[] associations)
            : this(service.Details, service.On, service.IsCancelled, associations)
        {
        }
        
        internal ResolvedServiceWithAssociations(Schedule service, DateTime on, bool isCancelled, ResolvedAssociation[] associations)
            : base(service, on, isCancelled)
        {
            Associations = associations;
        }
        
        public bool HasAssociations()
        {
            return Associations.Any();
        }
        
        public override string ToString()
        {
            return HasAssociations() ? $"{base.ToString()} +{Associations.Length}" : base.ToString();
        }
        
        private static ResolvedAssociation[] ResolveAssociations(string timetableUid, DateTime on, IDictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> associations)
        {
            if(associations == null)
                return new ResolvedAssociation[0];
          
            var resolvedAssociations = new List<ResolvedAssociation>();
            foreach (var versions in associations.Values)
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
                            var resolved = other.GetScheduleOn(on, false);
                            resolvedAssociations.Add(new ResolvedAssociation(association, on, isCancelled, resolved));
                            break;
                        }
                    }
                }
            }
            
            return  resolvedAssociations.ToArray();;
        }
    }
}