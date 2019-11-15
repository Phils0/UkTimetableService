using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public class ResolvedServiceWithAssociations : ResolvedService
    {
        private readonly Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> _associations;
        public ResolvedAssociation[] Associations { get; private set; } = new ResolvedAssociation[0];
        public bool FullyResolved { get; private set; } = false;

        public ResolvedServiceWithAssociations(ResolvedService service, Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> associations)
            : this(service.Details, service.On, service.IsCancelled, associations)
        {
        }

        public ResolvedServiceWithAssociations(Schedule service,DateTime on, bool isCancelled, Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> associations)
            : base(service, on, isCancelled)
        {
            _associations = associations;
        }

        public bool HasAssociations()
        {
            return Associations.Any();
        }
        
        public ResolvedServiceWithAssociations ResolveAssociations()
        {
            if(_associations == null)
            {
                FullyResolved = true;
                return this;
            }
            

            var associations = new List<ResolvedAssociation>();
            foreach (var versions in _associations.Values)
            {
                var isCancelled = false;
                foreach (var association in versions.Values)
                {
                    if (association.AppliesOn(On))
                    {
                        if (association.IsCancelled())
                            isCancelled = true;
                        else
                        {
                            var service = Details.TimetableUid == association.MainTimetableUid ? 
                                association.AssociatedService :
                                association.MainService;
                            var resolved = service.GetScheduleOn(On);
                            associations.Add(new ResolvedAssociation(association, On, isCancelled, resolved));
                            break;
                        }
                    }
                }
            }

            Associations = associations.ToArray();
            FullyResolved = true;
            return this;
        }
        
        public override string ToString()
        {
            return FullyResolved ? $"{base.ToString()} Not resolved" : 
                HasAssociations() ? $"{base.ToString()} +{Associations.Length}" : base.ToString();
        }
    }
}