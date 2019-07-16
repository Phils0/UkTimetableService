using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public interface ITimetable
    {
        (Schedule schedule, string reason) GetScheduleByTimetableUid(string timetableUid, DateTime date);
        (Schedule[] schedule, string reason) GetScheduleByRetailServiceId(string retailServiceId, DateTime date);
    }

    public class TimetableData : ITimetable
    {
        private Dictionary<string, Service> _timetableUidMap { get; } = new Dictionary<string, Service>(400000);
        private Dictionary<string, IList<Service>> _retailServiceIdMap { get; } = new Dictionary<string, IList<Service>>(400000);

        public void AddSchedule(Schedule schedule)
        {
            void AddToRetailServiceMap(Service trainService)
            {
                if (!_retailServiceIdMap.TryGetValue(schedule.RetailServiceId, out var services))
                {
                    services = new List<Service>();
                    _retailServiceIdMap.Add(schedule.RetailServiceId, services);
                }

                if (!services.Contains(trainService))
                    services.Add(trainService);
            }

            if (!_timetableUidMap.TryGetValue(schedule.TimetableUid, out var service))
            {
                service = new Service(schedule.TimetableUid);
                _timetableUidMap.Add(service.TimetableUid, service);
            }
            
            if(!string.IsNullOrEmpty(schedule.RetailServiceId))
                AddToRetailServiceMap(service);
            
            schedule.AddToService(service);
        }

        public (Schedule schedule, string reason) GetScheduleByTimetableUid(string timetableUid, DateTime date)
        {
            if (!_timetableUidMap.TryGetValue(timetableUid, out var service))
                return (null, $"{timetableUid} not found in timetable");

            var schedule = service.GetScheduleOn(date);
            
            if(schedule == null)
                return (null, $"{timetableUid} does not run on {date:d}");
 
            if(schedule.StpIndicator == StpIndicator.Cancelled)
                return (null, $"{timetableUid} cancelled in STP on {date:d}");

            
            return (schedule, "");
        }

        public (Schedule[] schedule, string reason) GetScheduleByRetailServiceId(string retailServiceId, DateTime date)
        {
            if (!_retailServiceIdMap.TryGetValue(retailServiceId, out var services))
                return (new Schedule[0], $"{retailServiceId} not found in timetable");

            var schedules = new List<Schedule>();
            
            foreach (var service in services)
            {
                var schedule = service.GetScheduleOn(date);
            
                if(schedule == null)
                    continue;
 
                if(schedule.StpIndicator == StpIndicator.Cancelled)
                    return (new Schedule[0], $"{retailServiceId} cancelled in STP on {date:d}");
 
                schedules.Add(schedule);
            }

            var reason = schedules.Any() ? "" : $"{retailServiceId} does not run on {date:d}";
            
            return (schedules.ToArray(), reason);
        }
    }
}