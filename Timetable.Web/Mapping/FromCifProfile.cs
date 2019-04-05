using System.Collections.Generic;
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
                .ForMember(d => d.On, o => o.ConvertUsing(new CalendarConverter(), s => s))
                .ForMember(d => d.RetailServiceId, o => o.Ignore())
                .ForMember(d => d.Toc, o => o.Ignore());            
            CreateMap<CifParser.Records.ScheduleExtraData, Timetable.Schedule>()
                .ForMember(d => d.RetailServiceId, o => o.MapFrom(s => s.RetailServiceId))
                .ForMember(d => d.Toc, 
                    o => o.ConvertUsing(new TocConverter(new TocLookup(Log.Logger, new Dictionary<string, Toc>())), s => s.Toc))
                .ForAllOtherMembers(o => o.Ignore());         
            
            // Schedule location records
            
            CreateMap<CifParser.Schedule, Timetable.Schedule>()
                .ConvertUsing((s, d, c) => MapSchedule(s, c));
        }

        private Schedule MapSchedule(CifParser.Schedule source, ResolutionContext context)
        {
            void SetExtraDetails(Schedule schedule)
            {
                var extra = source.GetScheduleExtraDetails();
                if (extra == null)
                {
                    schedule.Toc = Toc.Unknown;
                    schedule.RetailServiceId = "";
                }
                else
                {
                    context.Mapper.Map(extra, schedule);
                }
            }
            
            var destination = context.Mapper.
                Map<CifParser.Records.ScheduleDetails, Timetable.Schedule>(source.GetScheduleDetails());
            SetExtraDetails(destination);

            return destination;
        }
    }
}