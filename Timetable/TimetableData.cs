using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

namespace Timetable
{
    public enum LookupStatus
    {
        Success,
        ServiceNotFound,
        NoScheduleOnDate,
        InvalidRetailServiceId,
        Error
    }
    
    public interface ITimetableLookup
    {
        delegate (LookupStatus status, ResolvedService[] services) GetServicesByToc(string toc, DateTime date, Time dayBoundary);
        
        (LookupStatus status, ResolvedService service) GetScheduleByTimetableUid(string timetableUid, DateTime date);
        (LookupStatus status, ResolvedService[] services) GetScheduleByRetailServiceId(string retailServiceId, DateTime date);
        (LookupStatus status, ResolvedService[] services) GetSchedulesByToc(string toc, DateTime date, Time dayBoundary);
        bool IsLoaded { get; }
    }

    public class TimetableData : ITimetableLookup
    {
        private readonly ILogger _logger;
        private Dictionary<string, IService> _timetableUidMap { get; } = new Dictionary<string, IService>(400000);
        private Dictionary<string, IList<IService>> _retailServiceIdMap { get; } = new Dictionary<string, IList<IService>>(400000);
        
        public TimetableData(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsLoaded { get; set; }
        
        public void AddSchedule(ISchedule schedule)
        {
            void AddToRetailServiceMap(IService trainService)
            {
                if (!_retailServiceIdMap.TryGetValue(schedule.NrsRetailServiceId, out var services))
                {
                    services = new List<IService>();
                    _retailServiceIdMap.Add(schedule.NrsRetailServiceId, services);
                }

                if (!services.Contains(trainService))
                    services.Add(trainService);
            }

            if (!_timetableUidMap.TryGetValue(schedule.TimetableUid, out var service))
            {
                service = new CifService(schedule.TimetableUid, _logger);
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
            
            if(service.TryResolveOn(date, out var schedule))
                return (LookupStatus.Success, schedule);
            
            return (LookupStatus.NoScheduleOnDate, null);
        }
        
        private Regex RetailServiceIdRegex = new Regex(@"^(\w\w\d{4})\d{0,2}");

        private bool TryToNormaliseRetailServiceId(ref string retailServiceId)
        {
            if (string.IsNullOrEmpty(retailServiceId))
                return false;

            var match = RetailServiceIdRegex.Match(retailServiceId);
            if (match.Success)
                retailServiceId = match.Groups[1].Value;
            return match.Success;
        }
        
        public (LookupStatus status, ResolvedService[] services) GetScheduleByRetailServiceId(string retailServiceId, DateTime date)
        {
            if(!TryToNormaliseRetailServiceId(ref retailServiceId))
                return (LookupStatus.InvalidRetailServiceId, new ResolvedService[0]);
                
            if (!_retailServiceIdMap.TryGetValue(retailServiceId, out var services))
                return (LookupStatus.ServiceNotFound, new ResolvedService[0]);

            var schedules = new List<ResolvedService>();
            
            foreach (var service in services)
            {
                if(service.TryResolveOn(date, out var schedule) && schedule.HasRetailServiceId(retailServiceId))
                    schedules.Add(schedule);
            }
            
            var servicesToReturn = ServiceFilter.Filter(schedules, true);
            var reason = servicesToReturn.Any() ? LookupStatus.Success : LookupStatus.NoScheduleOnDate;
            return (reason, servicesToReturn);
        }



        public (LookupStatus status, ResolvedService[] services) GetSchedulesByToc(string toc, DateTime date, Time dayBoundary)
        {
            var services = new List<ResolvedService>();
            var nextDay = date.AddDays(1);
            
            foreach (var service in _timetableUidMap.Values)
            {
                try
                {
                    var onDate = IsNextDay(service) ? nextDay : date;
                    if (service.TryResolveOn(onDate, out var schedule) && schedule.OperatedBy(toc))
                        services.Add(schedule);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error processing {service}", service);
                }
            }

            var reason = services.Any() ? LookupStatus.Success : LookupStatus.ServiceNotFound;
            return (reason, services.ToArray());
            
            bool IsNextDay(IService service) => service.StartsBefore(dayBoundary);
        }
        
        public int AddAssociations(IEnumerable<Association> associations)
        {
            int count = 0;
            
            void Add(Association association)
            {
                if (_timetableUidMap.TryGetValue(association.Main.TimetableUid, out var mainService) &&
                    _timetableUidMap.TryGetValue(association.Associated.TimetableUid, out var otherService))
                {
                    if (mainService.AddAssociation(association, true))
                    {
                        otherService.AddAssociation(association, false);
                        count++;                        
                    }
                }
                else
                {
                    var which = mainService == null ? "main" : "associated";
                    _logger.Information("Could not add {association} did not find {which}", association, which);
                }
            }

            foreach (var association in associations)
            {
                Add(association);
            }

            return count;
        }
    }
}