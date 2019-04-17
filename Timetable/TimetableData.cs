using System;
using System.Collections.Generic;

namespace Timetable
{
    public interface ITimetable
    {
        Schedule GetSchedule(string timetableUid, DateTime date);
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

        public Schedule GetSchedule(string timetableUid, DateTime date)
        {
            return _data.TryGetValue(timetableUid, out var service) ? service.GetScheduleOn(date) : null;
        }
    }
}