using Serilog;
using Timetable.Web.Model;

namespace Timetable.Web.Mapping
{
    public class ResolvedServiceToSummaryConverter : ResolvedServiceConverter<Model.ServiceSummary, Model.FoundSummaryItem, Model.AssociationSummary>
    {

        internal ResolvedServiceToSummaryConverter(ILogger logger) : base(logger)
        {
        }
        
        protected override void SetAssociations(ServiceSummary service, AssociationSummary[] associations)
        {
            service.Associations = associations;
        }

        protected override void SetAssociatedService(AssociationSummary association, ServiceSummary service)
        {
            association.AssociatedService = service;
        }

        protected override void SetService(FoundSummaryItem item, ServiceSummary service)
        {
            item.Service = service;
        }
    }
}