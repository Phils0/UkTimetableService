using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CifParser;
using CifParser.Records;
using Serilog;

namespace Timetable.Web.Mapping
{
    public class ScheduleConverter : ITypeConverter<CifParser.Schedule, Timetable.Schedule>
    {
        private readonly ILogger _logger;

        public ScheduleConverter(ILogger logger)
        {
            _logger = logger;
        }

        public Schedule Convert(CifParser.Schedule source, Schedule destination, ResolutionContext context)
        {
            bool SetExtraDetails(Schedule schedule)
            {
                var extra = source.GetScheduleExtraDetails();
                if (extra == null)
                {
                    schedule.Toc = Toc.Unknown;
                    schedule.RetailServiceId = "";
                    return false;
                }
                else
                {
                    context.Mapper.Map(extra, schedule, context);
                    return true;
                }
            }

            List<IScheduleLocation> MapLocations(IEnumerable<IRecord> records)
            {
                var locations = new List<IScheduleLocation>();
                foreach (var record in records)
                {
                    IScheduleLocation working = null;
                    
                    switch (record)
                    {
                        case IntermediateLocation il:
                            working = MapLocation(il);
                            break;
                        case OriginLocation ol:
                            working = MapOrigin(ol);
                            break;
                        case TerminalLocation tl:
                            working = MapDestination(tl);
                            break;
                        case ScheduleChange sc:
                            _logger.Debug("Unhandled ScheduleChange : {record}", sc);
                            break;
                        default:
                            _logger.Warning("Unhandled record {recordType} : {record}", record.GetType(), record);
                            break;
                    }
                    
                    if(working?.Location != null)
                        locations.Add(working);
                }

                return locations;
            }

            IScheduleLocation MapLocation(IntermediateLocation il)
            {
                return il.WorkingPass == null
                    ? (IScheduleLocation) context.Mapper.Map<IntermediateLocation, ScheduleStop>(il, null, context)
                    : context.Mapper.Map<IntermediateLocation, SchedulePass>(il, null, context);
            }

            IScheduleLocation MapOrigin(OriginLocation ol)
            {
                return context.Mapper.Map<OriginLocation, ScheduleOrigin>(ol, null, context);
            }

            IScheduleLocation MapDestination(TerminalLocation tl)
            {
                return context.Mapper.Map<TerminalLocation, ScheduleDestination>(tl, null, context);
            }

            var d = context.Mapper.Map<CifParser.Records.ScheduleDetails, Timetable.Schedule>(
                source.GetScheduleDetails());

            var skipTwo = SetExtraDetails(d);
            var l = MapLocations(source.Records.Skip(skipTwo ? 2 : 1));
            d.Locations = l.ToArray();

            return d;
        }
    }
}