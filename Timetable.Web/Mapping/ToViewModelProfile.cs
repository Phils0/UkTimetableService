using System;
using AutoMapper;

namespace Timetable.Web.Mapping
{
    public class ToViewModelProfile : Profile
    {
        public ToViewModelProfile()
        {
            CreateMap<Timetable.Coordinates, Model.Coordinates>();
            CreateMap<Timetable.Location, Model.Location>();
            CreateMap<Timetable.Station, Model.Station>();

            CreateMap<Timetable.Location, Model.ScheduleLocation>();
            CreateMap<Timetable.ScheduleOrigin, Model.ScheduledStop>()
                .ForMember(d => d.Arrival, o => o.Ignore())
                .ForMember(d => d.PassesAt, o => o.Ignore());
            CreateMap<Timetable.ScheduleStop, Model.ScheduledStop>()
                .ForMember(d => d.PassesAt, o => o.Ignore());
            CreateMap<Timetable.ScheduleDestination, Model.ScheduledStop>()
                .ForMember(d => d.Departure, o => o.Ignore())
                .ForMember(d => d.PassesAt, o => o.Ignore());
            CreateMap<Timetable.SchedulePass, Model.ScheduledStop>()
                .ForMember(d => d.Arrival, o => o.Ignore())
                .ForMember(d => d.Departure, o => o.Ignore());
            CreateMap<Timetable.IScheduleLocation, Model.ScheduledStop>()
                .ConvertUsing((s, d, c) => ConvertToStop(s, c));
            CreateMap<Timetable.Schedule, Model.Service>()
                .ForMember(d => d.Date, 
                    o => o.MapFrom((s, d, dm, c) => c.Items["On"]))
                .ForMember(d => d.Stops, o => o.MapFrom(s => s.Locations));
        }

        private Model.ScheduledStop ConvertToStop(IScheduleLocation scheduleLocation, ResolutionContext context)
        {
            return (Model.ScheduledStop) context.Mapper
                .Map(scheduleLocation, scheduleLocation.GetType(), typeof(Model.ScheduledStop));
        }
    }
}