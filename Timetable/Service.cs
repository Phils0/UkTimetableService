using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Timetable
{
    public class Service
    {
        private sealed class StpDescendingComparer : IComparer<(StpIndicator indicator, ICalendar calendar)>
        {
            public int Compare((StpIndicator indicator, ICalendar calendar) x,
                (StpIndicator indicator, ICalendar calendar) y)
            {
                var compare = y.indicator.CompareTo(x.indicator);
                return compare != 0 ? compare : x.calendar.CompareTo(y.calendar);
            }
        }
        public string TimetableUid { get; }

        private readonly SortedList<(StpIndicator indicator, ICalendar calendar), Schedule> _schedules =
            new SortedList<(StpIndicator indicator, ICalendar calendar), Schedule>(new StpDescendingComparer());

        public Service(Schedule schedule)
        {
            _schedules.Add((schedule.StpIndicator, schedule.Calendar), schedule);
            TimetableUid = schedule.TimetableUid;
        }

        public void Add(Schedule schedule)
        {
            if (schedule.TimetableUid != TimetableUid)
                throw new ArgumentException(
                    $"Service: {TimetableUid}  TimetableUID does not match. Failed to add schedule: {schedule}");

            _schedules.Add((schedule.StpIndicator, schedule.Calendar), schedule);
        }

        public Schedule GetScheduleOn(DateTime date)
        {
            foreach (var schedule in _schedules.Values)
            {
                if (schedule.RunsOn(date))
                    return schedule;
            }
            return null;
        }

        public override string ToString()
        {
            return TimetableUid;
        }
    }
}