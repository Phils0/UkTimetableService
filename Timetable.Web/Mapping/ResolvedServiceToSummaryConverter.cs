using System;
using System.Collections.Generic;
using AutoMapper;
using Serilog;
using Timetable.Web.Model;

namespace Timetable.Web.Mapping
{
    public class ResolvedServiceToSummaryConverter : 
        ITypeConverter<Timetable.ResolvedService, Model.ServiceSummary>,  
        ITypeConverter<Timetable.ResolvedService[], Model.ServiceSummary[]>,
        ITypeConverter<Timetable.ResolvedServiceStop, Model.FoundSummaryItem>
    {
        private readonly ILogger _logger;

        internal ResolvedServiceToSummaryConverter(ILogger logger)
        {
            _logger = logger;
        }
        
        public Model.ServiceSummary Convert(ResolvedService source, Model.ServiceSummary destination, ResolutionContext context)
        {
            if (source is Timetable.ResolvedServiceWithAssociations withAssociations)
                return MapServiceWithAssociations(withAssociations, context);
            return  CreateService(source, context);
        }
        
        private Model.ServiceSummary CreateService(ResolvedService source, ResolutionContext context)
        {
            var service = context.Mapper.Map<Model.ServiceSummary>(source.Details, opts => opts.Items["On"] = source.On);
            service.Date = source.On;
            service.IsCancelled = source.IsCancelled;
            return service;
        }

        private Model.ServiceSummary MapServiceWithAssociations(Timetable.ResolvedServiceWithAssociations source, ResolutionContext context)
        {
            var thisService = CreateService(source, context);
            var associations = new List<Model.AssociationSummary>();
            foreach (var sourceAssociation in source.Associations)
            {
                try
                {
                    var mapped = MapAssociation(sourceAssociation, source, context);
                    associations.Add(mapped);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed to add association : {sourceAssociation}", sourceAssociation);
                }
            }

            thisService.Associations = associations.ToArray();
            return thisService;
        }
        
        private Model.AssociationSummary MapAssociation(ResolvedAssociation source, ResolvedService service, ResolutionContext context)
        {
            var atStop = Map(source.GetStop(service), context);
            var (associatedService, associatedStop) = MapOtherService();

            var association = new Model.AssociationSummary()
            {
                Stop =  atStop,
                IsCancelled = source.IsCancelled,
                IsMain = source.IsMain(service.TimetableUid),
                Date = source.On,
                AssociationCategory = source.Details.Category.ToString(),
                AssociatedService = associatedService,
                AssociatedServiceStop = associatedStop
            };
            
            return association;

            (Model.ServiceSummary, Model.ScheduledStop) MapOtherService()
            {
                // Remember original service date as can change
                var originalDate = context.Items["On"];
                try
                {
                    var otherService = CreateService(source.AssociatedService, context);
                    var otherStop = Map(source.GetStop(source.AssociatedService), context);
                    return (otherService, otherStop);
                }
                finally
                {
                    // Reset service date
                    context.Items["On"] = originalDate;
                }
            }
        }

        private Model.ScheduledStop Map(Timetable.ScheduleLocation stop, ResolutionContext context)
        {
            return context.Mapper.Map<Timetable.ScheduleLocation,Model.ScheduledStop>(stop);
        }

        public Model.ServiceSummary[] Convert(ResolvedService[] source, Model.ServiceSummary[] destination, ResolutionContext context)
        {
            var services = new List<Model.ServiceSummary>();
            foreach (var sourceService in source)
            {
                services.Add(Convert(sourceService, null, context));
            }
            
            return services.ToArray();
        }

        public FoundSummaryItem Convert(ResolvedServiceStop source, FoundSummaryItem destination, ResolutionContext context)
        {
            return new FoundSummaryItem()
            {
                Service = CreateService(source, context),
                At = context.Mapper.Map<Model.ScheduledStop>(source.Stop, opts => opts.Items["On"] = source.On),
                To = context.Mapper.Map<Model.ScheduledStop>(source.FoundToStop, opts => opts.Items["On"] = source.On),
                From = context.Mapper.Map<Model.ScheduledStop>(source.FoundFromStop, opts => opts.Items["On"] = source.On)
            };
        }
    }
}