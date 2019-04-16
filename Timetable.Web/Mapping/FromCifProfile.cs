using System;
using AutoMapper;
using Serilog;

namespace Timetable.Web.Mapping
{
    public class FromCifProfile : Profile
    {
        public FromCifProfile()
        {
            CreateMap<CifParser.RdgRecords.Station, Timetable.Location>().
                ForPath(d =>d.Coordinates.Eastings, o => o.MapFrom(s => s.East)).
                ForPath(d => d.Coordinates.Northings, o => o.MapFrom(s => s.North)).
                ForPath(d => d.Coordinates.IsEstimate, o => o.MapFrom(s => s.PositionIsEstimated)).
                ForMember(d => d.Station, o => o.Ignore()).
                ForMember(d => d.Nlc, o => o.Ignore());
                       
            // Schedule records
            CreateMap<CifParser.Records.ScheduleDetails, Timetable.Schedule>()
                .ForMember(d => d.Calendar, o => o.ConvertUsing(new CalendarConverter(), s => s))
                .ForMember(d => d.RetailServiceId, o => o.Ignore())
                .ForMember(d => d.Toc, o => o.Ignore())
                .ForMember(d => d.Locations, o => o.Ignore());
            CreateMap<CifParser.Records.ScheduleExtraData, Timetable.Schedule>()
                .ForMember(d => d.RetailServiceId, o => o.MapFrom(s => s.RetailServiceId))
                .ForMember(d => d.Toc, o => o.ConvertUsing(new TocConverter(), s => s.Toc))
                .ForAllOtherMembers(o => o.Ignore());

            CreateMap<TimeSpan, Time>()
                .ConvertUsing(t => new Time(t));
            // Schedule location records
            var locationConverter = new LocationsConverter();
            CreateMap<CifParser.Records.OriginLocation, ScheduleOrigin>()
                .ForMember(d => d.Departure, o => o.MapFrom(s => s.PublicDeparture))
                .ForMember(d => d.Location, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.Activities, o => o.MapFrom(s => Activities.Split(s.Activities)));
            CreateMap<CifParser.Records.IntermediateLocation, ScheduleStop>()
                .ForMember(d => d.Arrival, o => o.MapFrom(s => s.PublicArrival))
                .ForMember(d => d.Departure, o => o.MapFrom(s => s.PublicDeparture))
                .ForMember(d => d.Location, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.Activities, o => o.MapFrom(s => Activities.Split(s.Activities)));
            CreateMap<CifParser.Records.IntermediateLocation, SchedulePass>()
                .ForMember(d => d.Location, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.PassesAt, o => o.MapFrom(s => s.WorkingPass))
                .ForMember(d => d.Activities, o => o.MapFrom(s => Activities.Split(s.Activities)));
            CreateMap<CifParser.Records.TerminalLocation, ScheduleDestination>()
                .ForMember(d => d.Arrival, o => o.MapFrom(s => s.PublicArrival))
                .ForMember(d => d.Location, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.Activities, o => o.MapFrom(s => Activities.Split(s.Activities)));
            
            CreateMap<CifParser.Schedule, Timetable.Schedule>()
                .ConvertUsing(new ScheduleConverter(Log.Logger));
            
            
        }

    }
}