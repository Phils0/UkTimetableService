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

        private Schedule _schedule;
        
        private SortedList<(StpIndicator indicator, ICalendar calendar), Schedule> _multipleSchedules;

        public Service(Schedule schedule)
        {
            _schedule = schedule;
            TimetableUid = schedule.TimetableUid;
        }

        public void Add(Schedule schedule)
        {
            if (schedule.TimetableUid != TimetableUid)
                throw new ArgumentException(
                    $"Service: {TimetableUid}  TimetableUID does not match. Failed to add schedule: {schedule}");

            if (_schedule != null)
                MoveToSortedList();
            _multipleSchedules.Add((schedule.StpIndicator, schedule.Calendar), schedule);
        }

        private void MoveToSortedList()
        {
            _multipleSchedules =
                new SortedList<(StpIndicator indicator, ICalendar calendar), Schedule>(new StpDescendingComparer());
            _multipleSchedules.Add((_schedule.StpIndicator, _schedule.Calendar), _schedule);
            _schedule = null;
        }

        public Schedule GetScheduleOn(DateTime date)
        {
            if (_schedule != null)
            {
                if (_schedule.RunsOn(date))
                    return _schedule;
            }
            
            foreach (var schedule in _multipleSchedules.Values)
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