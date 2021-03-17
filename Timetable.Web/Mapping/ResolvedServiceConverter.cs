using System;
using System.Collections.Generic;
using AutoMapper;
using Serilog;
using Serilog.Events;

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
            return CreateService(source, context);
        }

        private S CreateService(ResolvedService source, ResolutionContext context)
        {
            context.Items["On"] = source.On;
            var service = context.Mapper.Map<S>(source.Details);
            service.Date = source.On;
            service.IsCancelled = source.IsCancelled;
            return service;
        }

        private S MapServiceWithAssociations(Timetable.ResolvedServiceWithAssociations source,
            ResolutionContext context)
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
            var (associatedService, associatedServiceStop) = MapOtherService();

            var association = new A()
            {
                Stop = MapStop(source.Stop.Stop, context),
                IsBroken = source.Stop.IsBroken,
                IsCancelled = source.IsCancelled,
                IsMain = source.IsMain(service.TimetableUid),
                Date = source.On,
                AssociationCategory = source.Details.Category.ToString(),
                AssociatedServiceStop = associatedServiceStop
            };
            SetAssociatedService(association, associatedService);
            return association;

            (S, Model.ScheduledStop) MapOtherService()
            {
                // Association maybe on different day.
                var originalDate = context.Items["On"];
                try
                {
                    var otherService = CreateService(source.AssociatedService, context);
                    var otherStop = MapStop(source.Stop.AssociatedServiceStop, context);
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

        private Model.ScheduledStop MapStop(ResolvedStop source, ResolutionContext context)
        {
            return source == null ?
                null :
                context.Mapper.Map<Timetable.ScheduleLocation, Model.ScheduledStop>(source.Stop);
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
            // var original = context.Items["On"];
            // context.Items["On"] = source.On;
            var item = new FS()
            {
                At = context.Mapper.Map<Model.ScheduledStop>(source.Stop),
                To = context.Mapper.Map<Model.ScheduledStop>(source.FoundToStop),
                From = context.Mapper.Map<Model.ScheduledStop>(source.FoundFromStop),
                Association = context.Mapper.Map<Model.IncludedAssociation>(source.Association),
                ViaText = source.ViaText
            };
            var service = Convert(source.Service, (S) null, context);
            SetService(item, service);

            return item;
        }

        protected abstract void SetService(FS item, S service);
    }
}