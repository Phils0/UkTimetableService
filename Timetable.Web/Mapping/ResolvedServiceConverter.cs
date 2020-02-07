using System;
using System.Collections.Generic;
using AutoMapper;
using Serilog;

namespace Timetable.Web.Mapping
{
        public abstract class ResolvedServiceConverter<S, FS, A> : 
            ITypeConverter<Timetable.ResolvedService, S>,  
            ITypeConverter<Timetable.ResolvedService[], S[]>,
            ITypeConverter<Timetable.ResolvedServiceStop, FS>
        where S : Model.ServiceBase, new()
        where FS : Model.FoundItem, new()
        where A : Model.AssociationBase, new()
    {
        private readonly ILogger _logger;

        internal ResolvedServiceConverter(ILogger logger)
        {
            _logger = logger;
        }
        
        public S Convert(ResolvedService source, S destination, ResolutionContext context)
        {
            if (source is Timetable.ResolvedServiceWithAssociations withAssociations)
                return MapServiceWithAssociations(withAssociations, context);
            return  CreateService(source, context);
        }
        
        private S CreateService(ResolvedService source, ResolutionContext context)
        {
            var service = context.Mapper.Map<S>(source.Details, opts => opts.Items["On"] = source.On);
            service.Date = source.On;
            service.IsCancelled = source.IsCancelled;
            return service;
        }

        private S MapServiceWithAssociations(Timetable.ResolvedServiceWithAssociations source, ResolutionContext context)
        {
            var thisService = CreateService(source, context);
            var associations = new List<A>();
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
            
            SetAssociations(thisService, associations.ToArray());
            return thisService;
        }

        protected abstract void SetAssociations(S service, A[] associations);
        
        private A MapAssociation(ResolvedAssociation source, ResolvedService service, ResolutionContext context)
        {
            var atStop = Map(source.GetStop(service).Stop, context);
            var (associatedService, associatedStop) = MapOtherService();

            var association = new A()
            {
                Stop =  atStop,
                IsCancelled = source.IsCancelled,
                IsMain = source.IsMain(service.TimetableUid),
                Date = source.On,
                AssociationCategory = source.Details.Category.ToString(),
                AssociatedServiceStop = associatedStop
            };
            SetAssociatedService(association, associatedService);
            return association;

            (S, Model.ScheduledStop) MapOtherService()
            {
                // Remember original service date as can change
                var originalDate = context.Items["On"];
                try
                {
                    var otherService = CreateService(source.AssociatedService, context);
                    var otherStop = Map(source.GetStop(source.AssociatedService).Stop, context);
                    return (otherService, otherStop);
                }
                finally
                {
                    // Reset service date
                    context.Items["On"] = originalDate;
                }
            }
        }

        protected abstract void SetAssociatedService(A association, S service);
        
        private Model.ScheduledStop Map(Timetable.ScheduleLocation stop, ResolutionContext context)
        {
            return context.Mapper.Map<Timetable.ScheduleLocation,Model.ScheduledStop>(stop);
        }

        public S[] Convert(ResolvedService[] source, S[] destination, ResolutionContext context)
        {
            var services = new List<S>();
            foreach (var sourceService in source)
            {
                services.Add(Convert(sourceService, null, context));
            }
            
            return services.ToArray();
        }

        public FS Convert(ResolvedServiceStop source, FS destination, ResolutionContext context)
        {
            var item = new FS()
            {
                At = context.Mapper.Map<Model.ScheduledStop>(source.Stop, opts => opts.Items["On"] = source.On),
                To = context.Mapper.Map<Model.ScheduledStop>(source.FoundToStop, opts => opts.Items["On"] = source.On),
                From = context.Mapper.Map<Model.ScheduledStop>(source.FoundFromStop, opts => opts.Items["On"] = source.On),
                Association = context.Mapper.Map<Model.IncludedAssociation>(source.Association)
            };
            var service = Convert(source.Service, (S) null, context);
            SetService(item, service);
            
            return item;
        }

        protected abstract void SetService(FS item, S service);
    }
}