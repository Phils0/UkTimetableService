using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Serilog;

namespace Timetable.Web.Mapping
{
    public class ToViewModelProfile : Profile
    {
        public ToViewModelProfile()
        {
            CreateMap<Timetable.OrdnanceSurveyCoordinates, Model.OrdnanceSurveyCoordinates>();
            CreateMap<Timetable.Location, Model.Location>();
            CreateMap<Timetable.Toc, string>()
                .ConvertUsing((s, d, c) => s?.Code );
            CreateMap<Timetable.Coordinates, Model.Coordinates>();
            CreateMap<Timetable.Station, Model.Station>();
            CreateMap<Timetable.Toc, Model.Toc>();
            
            CreateMap<Timetable.Location, Model.LocationId>();
            CreateMap<Timetable.ScheduleStop, Model.ScheduledStop>()
                .ForMember(d => d.Arrival,o => o.MapFrom((s, d, dm, c) => ResolveTime(s.Arrival, c)))
                .ForMember(d => d.Departure,o => o.MapFrom((s, d, dm, c) => ResolveTime(s.Departure, c)))
                .ForMember(d => d.PassesAt, o => o.Ignore())
                .ForMember(d => d.Activities, o => o.MapFrom((s, d) => s.Activities.Value));
            CreateMap<Timetable.SchedulePass, Model.ScheduledStop>()
                .ForMember(d => d.Arrival, o => o.Ignore())
                .ForMember(d => d.Departure, o => o.Ignore())
                .ForMember(d => d.PassesAt,o => o.MapFrom((s, d, dm, c) => ResolveTime(s.PassesAt, c)))
                .ForMember(d => d.Activities, o => o.MapFrom((s, d) => s.Activities.Value));
            CreateMap<Timetable.ScheduleLocation, Model.ScheduledStop>()
                .ConvertUsing((s, d, c) => ConvertToStop(s, c));
            CreateMap<Timetable.ResolvedStop, Model.ScheduledStop>()
                .ConvertUsing((s, d, c) => ConvertToStop(s, c));
            CreateMap<Timetable.IScheduleProperties, Model.Service>()
                .ForMember(d => d.NrsRetailServiceId, o => o.MapFrom((s, d) => s.ShortRetailServiceId))
                .ForMember(d => d.TimetableUid, o => o.Ignore())
                .ForMember(d => d.Date, o => o.Ignore())
                .ForMember(d => d.IsCancelled, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.Associations, o => o.Ignore())
                .ForMember(d => d.Stops, o => o.Ignore());
            CreateMap<Timetable.ISchedule, Model.Service>()
                .IncludeMembers(s => s.Properties)
                .ForMember(d => d.Date, o => o.Ignore())
                .ForMember(d => d.IsCancelled, o => o.Ignore())
                .ForMember(d => d.Associations, o => o.Ignore())
                .ForMember(d => d.Stops, o => o.MapFrom((s, d, dm, c) => MapStops(s.Locations, c)));
            CreateMap<Timetable.IScheduleProperties, Model.ServiceSummary>()
                .ForMember(d => d.NrsRetailServiceId, o => o.MapFrom((s, d) => s.ShortRetailServiceId))
                .ForMember(d => d.TimetableUid, o => o.Ignore())
                .ForMember(d => d.Date, o => o.Ignore())
                .ForMember(d => d.IsCancelled, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.Associations, o => o.Ignore())
                .ForMember(d => d.Origin, o => o.Ignore())
                .ForMember(d => d.Destination, o => o.Ignore());
            CreateMap<Timetable.ISchedule, Model.ServiceSummary>()
                .IncludeMembers(s => s.Properties)
                .ForMember(d => d.Date, o => o.Ignore())
                .ForMember(d => d.IsCancelled, o => o.Ignore())
                .ForMember(d => d.Origin, o => o.MapFrom(s => s.Locations.First()))
                .ForMember(d => d.Destination, o => o.MapFrom(s => s.Locations.Last()))
                .ForMember(d => d.Associations, o => o.Ignore());
            CreateMap<Timetable.IncludedAssociation, Model.IncludedAssociation>();
            
            var serviceConverter = new ResolvedServiceToServiceConverter(Log.Logger);
            CreateMap<Timetable.ResolvedService, Model.Service>()
                .ConvertUsing(serviceConverter);
            CreateMap<Timetable.ResolvedService[], Model.Service[]>()
                .ConvertUsing(serviceConverter);
             CreateMap<Timetable.ResolvedServiceStop, Model.FoundServiceItem>()
                 .ConvertUsing(serviceConverter);   
             
            var summaryConverter = new ResolvedServiceToSummaryConverter(Log.Logger);
            CreateMap<Timetable.ResolvedService, Model.ServiceSummary>()
                .ConvertUsing(summaryConverter);        
            CreateMap<Timetable.ResolvedServiceStop, Model.FoundSummaryItem>()
                .ConvertUsing(summaryConverter);

        }

        private static DateTime? ResolveTime(Time time, ResolutionContext context)
        {
            if (!time.IsValid)
                return null;
            
            var date = (DateTime) context.Items["On"];
            return date.Add(time.Value);
        }
        
        private Model.ScheduledStop[] MapStops(IReadOnlyList<ScheduleLocation> source, ResolutionContext context)
        {
            return source.Select(s => ConvertToStop(s, context)).ToArray();
        }
        private Model.ScheduledStop ConvertToStop(ScheduleLocation scheduleLocation, ResolutionContext context)
        {
            return (Model.ScheduledStop) context.Mapper
                .Map(scheduleLocation, scheduleLocation.GetType(), typeof(Model.ScheduledStop));
        }
        
        private Model.ScheduledStop ConvertToStop(ResolvedStop stop, ResolutionContext context)
        {
            context.Items["On"] = stop.On;
            return (Model.ScheduledStop) context.Mapper
                .Map(stop.Stop, stop.Stop.GetType(), typeof(Model.ScheduledStop));
        }
    }
}