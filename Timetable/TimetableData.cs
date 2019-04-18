using System;
using System.Collections.Generic;

namespace Timetable
{
    public interface ITimetable
    {
        (Schedule schedule, string reason) GetSchedule(string timetableUid, DateTime date);
    }

    public class TimetableData : ITimetable
    {
        private Dictionary<string, Service> _data { get; } = new Dictionary<string, Service>(400000);

        public void Add(Schedule schedule)
        {
            if (_data.TryGetValue(schedule.TimetableUid, out var service))
            {
                service.Add(schedule);
            }
            else
            {
                service = new Service(schedule);
                _data.Add(service.TimetableUid, service);
            }
        }

        public (Schedule schedule, string reason) GetSchedule(string timetableUid, DateTime date)
        {
            if (!_data.TryGetValue(timetableUid, out var service))
                return (null, $"{timetableUid} not found in timetable");

            var schedule = service.GetScheduleOn(date);
            
            if(schedule == null)
                return (null, $"{timetableUid} does not run on {date:d}");
 
            if(schedule.StpIndicator == StpIndicator.Cancelled)
                return (null, $"{timetableUid} cancelled in STP on {date:d}");

            
            return (schedule, "");
        }
    }
}