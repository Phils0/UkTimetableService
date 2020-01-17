using Serilog;

namespace Timetable.Web.Mapping
{
    public class ResolvedServiceToServiceConverter : ResolvedServiceConverter<Model.Service, Model.FoundServiceItem, Model.Association>
    {

        internal ResolvedServiceToServiceConverter(ILogger logger) : base(logger)
        {
        }
        
        protected override void SetAssociations(Model.Service service, Model.Association[] associations)
        {
            service.Associations = associations;
        }

        protected override void SetAssociatedService(Model.Association association, Model.Service service)
        {
            association.AssociatedService = service;
        }

        protected override void SetService(Model.FoundServiceItem item, Model.Service service)
        {
            item.Service = service;
        }
    }
}