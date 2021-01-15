using System;
using System.Linq;

namespace Timetable
{
    public class ResolvedServiceWithAssociations : ResolvedService
    {
        public ResolvedAssociation[] Associations { get; }
        
        public ResolvedServiceWithAssociations(ResolvedService service, ResolvedAssociation[] associations)
            : this(service.Details, service.On, service.IsCancelled, associations)
        {
        }
        
        public ResolvedServiceWithAssociations(ISchedule service, DateTime on, bool isCancelled, ResolvedAssociation[] associations)
            : base(service, on, isCancelled)
        {
            Associations = associations;
        }
        
        public bool HasAssociations()
        {
            return Associations?.Any() ?? false;
        }
        
        public override string ToString()
        {
            return HasAssociations() ? $"{base.ToString()} +{Associations.Length}" : base.ToString();
        }
    }
}