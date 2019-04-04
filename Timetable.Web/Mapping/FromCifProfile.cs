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
            
            // Calendar
            CreateMap<string, DaysFlag>().ConvertUsing((s) => CalendarConverter.MapMask(s));
            CreateMap<string, BankHolidayRunning>().ConvertUsing((s) => CalendarConverter.MapBankHoliday(s));
            CreateMap<CifParser.Records.ScheduleDetails, Timetable.Calendar>()
                .ForMember(d => d.BankHolidays, o => o.MapFrom(s => s.BankHolidayRunning));           
            // Schedule records
            CreateMap<CifParser.Records.ScheduleDetails, Timetable.Schedule>()
                .ForMember(d => d.On, o => o.ConvertUsing(new CalendarConverter(), s => s))
                .ForMember(d => d.RetailServiceId, o => o.Ignore())
                .ForMember(d => d.Toc, o => o.Ignore());            
            CreateMap<CifParser.Records.ScheduleExtraData, Timetable.Schedule>()
                .ForMember(d => d.RetailServiceId, o => o.MapFrom(s => s.RetailServiceId))
                .ForMember(d => d.Toc, 
                    o => o.ConvertUsing(new TocConverter(new TocLookup(Log.Logger)), s => s.Toc))
                .ForAllOtherMembers(o => o.Ignore());         
            
            CreateMap<CifParser.Schedule, Timetable.Schedule>()
                .ConvertUsing((s, d, c) => Map(s, c));
        }

        private Schedule Map(CifParser.Schedule source, ResolutionContext context)
        {
            var destination = context.Mapper.
                Map<CifParser.Records.ScheduleDetails, Timetable.Schedule>(source.GetScheduleDetails());

            var extra = source.GetScheduleExtraDetails();
            if (extra != null)
                context.Mapper.Map<CifParser.Records.ScheduleExtraData, Timetable.Schedule>(extra, destination);
            
            return destination;
        }
    }
}