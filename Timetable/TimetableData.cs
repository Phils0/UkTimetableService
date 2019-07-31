using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public enum LookupStatus
    {
        Success,
        ServiceNotFound,
        NoScheduleOnDate,
        CancelledService
    }
    
    public interface ITimetable
    {
        (LookupStatus status, Schedule schedule) GetScheduleByTimetableUid(string timetableUid, DateTime date);
        (LookupStatus status, Schedule[] schedule) GetScheduleByRetailServiceId(string retailServiceId, DateTime date);
        (LookupStatus status, Schedule[] schedules) GetSchedulesByToc(string toc, DateTime date);
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

        public (LookupStatus status, Schedule schedule) GetScheduleByTimetableUid(string timetableUid, DateTime date)
        {
            if (!_timetableUidMap.TryGetValue(timetableUid, out var service))
                return (LookupStatus.ServiceNotFound, null);

            var schedule = service.GetScheduleOn(date);
            
            if(schedule == null)
                return (LookupStatus.NoScheduleOnDate, null);
 
            if(schedule.StpIndicator == StpIndicator.Cancelled)
                return (LookupStatus.CancelledService, null);

            
            return (LookupStatus.Success , schedule);
        }

        public (LookupStatus status, Schedule[] schedule) GetScheduleByRetailServiceId(string retailServiceId, DateTime date)
        {
            if (!_retailServiceIdMap.TryGetValue(retailServiceId, out var services))
                return (LookupStatus.ServiceNotFound, new Schedule[0]);

            var schedules = new List<Schedule>();
            
            foreach (var service in services)
            {
                var schedule = service.GetScheduleOn(date);
            
                if(schedule == null)
                    continue;
 
                if(schedule.StpIndicator == StpIndicator.Cancelled)
                    return (LookupStatus.CancelledService, new Schedule[0]);
 
                if(schedule.HasRetailServiceId(retailServiceId))
                    schedules.Add(schedule);
            }

            var reason = schedules.Any() ? LookupStatus.Success : LookupStatus.NoScheduleOnDate;
            
            return (reason, schedules.ToArray());
        }

        public (LookupStatus status, Schedule[] schedules) GetSchedulesByToc(string toc, DateTime date)
        {
            var schedules = new List<Schedule>();

            foreach (var service in _timetableUidMap.Values)
            {
                if (service.TryGetScheduleOn(date, out var schedule))
                {
                    if(schedule.Operator.Equals(toc))
                        schedules.Add(schedule);
                }
            }

            var reason = schedules.Any() ? LookupStatus.Success : LookupStatus.ServiceNotFound;

            return (reason, schedules.ToArray());
        }
    }
}