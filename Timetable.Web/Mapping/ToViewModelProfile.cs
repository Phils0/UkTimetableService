using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Serilog;
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
                .ForMember(d => d.Associations, o => o.Ignore())
                .ForMember(d => d.Stops, o => o.MapFrom((s, d, dm, c) => MapStops(s.Locations, c)));
            CreateMap<Timetable.Schedule, Model.ServiceSummary>()
                .ForMember(d => d.Date, o => o.Ignore())
                .ForMember(d => d.IsCancelled, o => o.Ignore())
                .ForMember(d => d.Origin, o => o.MapFrom(s => s.Locations.First()))
                .ForMember(d => d.Destination, o => o.MapFrom(s => s.Locations.Last()))
                .ForMember(d => d.Associations, o => o.Ignore());
            
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

        private ScheduledStop[] MapStops(IReadOnlyList<ScheduleLocation> source, ResolutionContext context)
        {
            return source.Select(s => ConvertToStop(s, context)).ToArray();
        }
        private Model.ScheduledStop ConvertToStop(ScheduleLocation scheduleLocation, ResolutionContext context)
        {
            return (Model.ScheduledStop) context.Mapper
                .Map(scheduleLocation, scheduleLocation.GetType(), typeof(Model.ScheduledStop),
                    o => o.Items["On"] = context.Items["On"]);
        }
    }
}