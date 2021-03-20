using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public class ResolvedServiceWithAssociations : ResolvedService
    {
        public ResolvedAssociation[] Associations { get; private set; }
        
        public ResolvedServiceWithAssociations(ResolvedService service, ResolvedAssociation[] associations)
            : this(service.Details, service.On, service.IsCancelled, associations)
        {
        }
        
        public ResolvedServiceWithAssociations(ISchedule service, DateTime on, bool isCancelled, ResolvedAssociation[] associations)
            : base(service, on, isCancelled)
        {
            Associations = associations ?? new ResolvedAssociation[0];
            SetAssociationStop();
        }

        private void SetAssociationStop()
        {
            foreach (var association in Associations)
            {
                association.SetAssociationStop(this);
            }
        }
        
        public override bool HasAssociations()
        {
            return Associations?.Any() ?? false;
        }
        
        public override string ToString()
        {
            return HasAssociations() ? $"{base.ToString()} +{Associations.Length}" : base.ToString();
        }
        
        internal void RemoveCancelledAssociations()
        {
            var active = new List<ResolvedAssociation>();
            foreach (var association in Associations)
            {
                if(!(association.IsCancelled || association.AssociatedService.IsCancelled))
                {
                    active.Add(association);
                }
            }

            Associations = active.ToArray();
        }
    }
}