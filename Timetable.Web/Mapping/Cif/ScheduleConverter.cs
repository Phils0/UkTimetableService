using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CifParser;
using CifParser.Records;
using Serilog;

namespace Timetable.Web.Mapping.Cif
{
    internal class ScheduleConverter : ITypeConverter<CifParser.Schedule, Timetable.CifSchedule>
    {
        private readonly ILogger _logger;

        internal ScheduleConverter(ILogger logger)
        {
            _logger = logger;
        }

        public CifSchedule Convert(CifParser.Schedule source, CifSchedule destination, ResolutionContext context)
        {
            var timetable = context.Items["Timetable"] as TimetableData;

            var schedule = new CifSchedule();           
            schedule = context.Mapper
                    .Map<CifParser.Records.ScheduleDetails, Timetable.CifSchedule>(source.GetScheduleDetails(), schedule);
            
            var skipTwo = SetExtraDetails();

            // Only add to timetable after set both TimetableUid and RetailServiceId
            timetable.AddSchedule(schedule);

            MapLocations(source.Records.Skip(skipTwo ? 2 : 1));

            return schedule;
            
            bool SetExtraDetails()
            {
                var extra = source.GetScheduleExtraDetails();
                if (extra == null)
                {
                    schedule.Operator = Toc.Unknown;
                    schedule.RetailServiceId = "";
                    return false;
                }
                else
                {
                    context.Mapper.Map(extra, schedule);
                    return true;
                }
            }

            void MapLocations(IEnumerable<IRecord> records)
            {
                var locations = new List<ScheduleLocation>(16);
                var start = Time.NotValid;
                var sequence = new Sequence();
 
                foreach (var record in records)
                {
                    ScheduleLocation working = null;
                    
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
                            // _logger.Debug("Unhandled ScheduleChange : {record}", sc);
                            break;
                        default:
                            _logger.Warning("Unhandled record {recordType} : {record}", record.GetType(), record);
                            break;
                    }

                    // Only add stops that we care about i.e. in the MSL
                    if (working?.Location != null)
                    {
                        EnsureTimesGoToTheFuture(working);
                        working.SetParent(schedule);
                        working.Id = sequence.GetNext();
                    }
                }

                ScheduleLocation MapLocation(IntermediateLocation il)
                {
                    return il.WorkingPass == null
                        ? (ScheduleLocation) context.Mapper.Map<IntermediateLocation, ScheduleStop>(il, null)
                        : context.Mapper.Map<IntermediateLocation, SchedulePass>(il, null);
                }

                ScheduleLocation MapOrigin(OriginLocation ol)
                {
                    var origin = context.Mapper.Map<OriginLocation, ScheduleStop>(ol, null);
                    start = origin.Departure.IsBefore(origin.WorkingDeparture) ? origin.Departure : origin.WorkingDeparture;
                    return origin;
                }

                ScheduleLocation MapDestination(TerminalLocation tl)
                {
                    return context.Mapper.Map<TerminalLocation, ScheduleStop>(tl, null);
                }
                
                void EnsureTimesGoToTheFuture(ScheduleLocation scheduleLocation)
                {
                    if (start.Equals(Time.NotValid))
                        _logger.Warning($"ID: {scheduleLocation.Id} Have not set start: {schedule.TimetableUid}");
                    scheduleLocation.AddDay(start);
                }

            }
        }
    }
}