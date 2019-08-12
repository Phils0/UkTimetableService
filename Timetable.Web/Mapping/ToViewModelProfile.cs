using System;
using System.Linq;
using AutoMapper;
using Timetable.Web.Model;

namespace Timetable.Web.Mapping
{
    public class ToViewModelProfile : Profile
    {
        public ToViewModelProfile()
        {
            CreateMap<Timetable.Coordinates, Model.Coordinates>();
            CreateMap<Timetable.Location, Model.Location>();
            CreateMap<Timetable.Station, Model.Station>();

            CreateMap<Timetable.Location, Model.LocationId>();
            CreateMap<Timetable.ScheduleOrigin, Model.ScheduledStop>()               
                .ForMember(d => d.Arrival, o => o.Ignore())
                .ForMember(d => d.Departure,o => o.MapFrom((s, d, dm, c) => ResolveTime(s.Departure, c)))
                .ForMember(d => d.PassesAt, o => o.Ignore());
            CreateMap<Timetable.ScheduleStop, Model.ScheduledStop>()
                .ForMember(d => d.Arrival,o => o.MapFrom((s, d, dm, c) => ResolveTime(s.Arrival, c)))
                .ForMember(d => d.Departure,o => o.MapFrom((s, d, dm, c) => ResolveTime(s.Departure, c)))
                .ForMember(d => d.PassesAt, o => o.Ignore());
            CreateMap<Timetable.ScheduleDestination, Model.ScheduledStop>()
                .ForMember(d => d.Arrival,o => o.MapFrom((s, d, dm, c) => ResolveTime(s.Arrival, c)))
                .ForMember(d => d.Departure, o => o.Ignore())
                .ForMember(d => d.PassesAt, o => o.Ignore());
            CreateMap<Timetable.SchedulePass, Model.ScheduledStop>()
                .ForMember(d => d.Arrival, o => o.Ignore())
                .ForMember(d => d.Departure, o => o.Ignore())
                .ForMember(d => d.PassesAt,o => o.MapFrom((s, d, dm, c) => ResolveTime(s.PassesAt, c)));
            CreateMap<Timetable.ScheduleLocation, Model.ScheduledStop>()
                .ConvertUsing((s, d, c) => ConvertToStop(s, c));
            CreateMap<Timetable.Schedule, Model.Service>()
                .ForMember(d => d.Date, o => o.Ignore())
                .ForMember(d => d.IsCancelled, o => o.Ignore())
                .ForMember(d => d.Stops, o => o.MapFrom(s => s.Locations));
            CreateMap<Timetable.Schedule, Model.ServiceSummary>()
                .ForMember(d => d.Date, o => o.Ignore())
                .ForMember(d => d.IsCancelled, o => o.Ignore())
                .ForMember(d => d.Origin, o => o.MapFrom(s => s.Locations.First()))
                .ForMember(d => d.Destination, o => o.MapFrom(s => s.Locations.Last()));
            CreateMap<Timetable.ResolvedService, Model.Service>()
                .ConvertUsing(MapService);
            CreateMap<Timetable.ResolvedService, Model.ServiceSummary>()
                .ConvertUsing(MapServiceSummary);        
            CreateMap<Timetable.ResolvedServiceStop, Model.FoundItem>()
                .ConvertUsing(MapFoundService);
        }
        
        private static DateTime? ResolveTime(Time time, ResolutionContext context)
        {
            if (!time.IsValid)
                return null;
            
            var date = (DateTime) context.Items["On"];
            return date.Add(time.Value);
        }

        private Model.ScheduledStop ConvertToStop(ScheduleLocation scheduleLocation, ResolutionContext context)
        {
            return (Model.ScheduledStop) context.Mapper
                .Map(scheduleLocation, scheduleLocation.GetType(), typeof(Model.ScheduledStop),
                    o => { o.Items["On"] = context.Items["On"]; });
        }
        
        private Model.Service MapService(Timetable.ResolvedService source, Model.Service notUsed, ResolutionContext context)
        {
            SetDateInContext(source, context);
            var service = context.Mapper.Map<Model.Service>(source.Details);
            service.Date = source.On;
            service.IsCancelled = source.IsCancelled;
            return service;
        }

        private void SetDateInContext(ResolvedService source, ResolutionContext context)
        {
            context.Options.Items["On"] = source.On;
        }

        private Model.ServiceSummary MapServiceSummary(ResolvedService source, Model.ServiceSummary notUsed, ResolutionContext context)
        {
            SetDateInContext(source, context);
            return CreateServiceSummary(source, context);
        }

        private ServiceSummary CreateServiceSummary(ResolvedService source, ResolutionContext context)
        {
            var service = context.Mapper.Map<Model.ServiceSummary>(source.Details);
            service.Date = source.On;
            service.IsCancelled = source.IsCancelled;
            return service;
        }

        private FoundItem MapFoundService(Timetable.ResolvedServiceStop source, FoundItem notUsed, ResolutionContext context)
        {
            SetDateInContext(source, context);
            return new FoundItem()
            {
                Service = CreateServiceSummary(source, context),
                At = context.Mapper.Map<Model.ScheduledStop>(source.Stop),
                To = context.Mapper.Map<Model.ScheduledStop>(source.FoundToStop)
            };
        }
    }
}