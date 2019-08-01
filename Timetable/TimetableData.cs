using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public enum LookupStatus
    {
        Success,
        ServiceNotFound,
        NoScheduleOnDate
    }
    
    public interface ITimetable
    {
        (LookupStatus status, ResolvedService service) GetScheduleByTimetableUid(string timetableUid, DateTime date);
        (LookupStatus status, ResolvedService[] services) GetScheduleByRetailServiceId(string retailServiceId, DateTime date);
        (LookupStatus status, ResolvedService[] services) GetSchedulesByToc(string toc, DateTime date);
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

        public (LookupStatus status, ResolvedService service) GetScheduleByTimetableUid(string timetableUid, DateTime date)
        {
            if (!_timetableUidMap.TryGetValue(timetableUid, out var service))
                return (LookupStatus.ServiceNotFound, null);

            var schedule = service.GetScheduleOn(date);
            
            if(schedule == null)
                return (LookupStatus.NoScheduleOnDate, null);
            
            return (LookupStatus.Success ,schedule);
        }

        public (LookupStatus status, ResolvedService[] services) GetScheduleByRetailServiceId(string retailServiceId, DateTime date)
        {
            if (!_retailServiceIdMap.TryGetValue(retailServiceId, out var services))
                return (LookupStatus.ServiceNotFound, new ResolvedService[0]);

            var schedules = new List<ResolvedService>();
            
            foreach (var service in services)
            {
                var schedule = service.GetScheduleOn(date);
                if(schedule == null)
                    continue;
                
                if(schedule.HasRetailServiceId(retailServiceId))
                    schedules.Add(schedule);
            }

            var reason = schedules.Any() ? LookupStatus.Success : LookupStatus.NoScheduleOnDate;
            
            return (reason, schedules.ToArray());
        }

        public (LookupStatus status, ResolvedService[] services) GetSchedulesByToc(string toc, DateTime date)
        {
            var services = new List<ResolvedService>();

            foreach (var service in _timetableUidMap.Values)
            {
                if (service.TryGetScheduleOn(date, out var schedule))
                {
                    if(schedule.OperatedBy(toc))
                        services.Add(schedule);
                }
            }

            var reason = services.Any() ? LookupStatus.Success : LookupStatus.ServiceNotFound;

            return (reason, services.ToArray());
        }
    }
}