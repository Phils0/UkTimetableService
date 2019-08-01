using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Timetable
{
    public class Service
    {
        private enum Multiplicity
        {
            None,
            SingleSchedule,
            MultipleSchedules
        }
        
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

        private Multiplicity _stateType = Multiplicity.None;

        public Service(string timetableUid)
        {
            TimetableUid = timetableUid;
            
        }

        internal void Add(Schedule schedule)
        {
            if (schedule.TimetableUid != TimetableUid)
                throw new ArgumentException(
                    $"Service: {TimetableUid}  TimetableUID does not match. Failed to add schedule: {schedule}");

            if (_stateType == Multiplicity.None)
            {
                SetSingleSchedule();
                return;
            }                    
            else if (_stateType == Multiplicity.SingleSchedule)
            {
                MoveToMultipleSchedules();                
            }
            
            _multipleSchedules.Add((schedule.StpIndicator, schedule.Calendar), schedule);

            void SetSingleSchedule()
            {
                _schedule = schedule;
                _stateType = Multiplicity.SingleSchedule;
            }
            
            void MoveToMultipleSchedules()
            {
                _multipleSchedules =
                    new SortedList<(StpIndicator indicator, ICalendar calendar), Schedule>(new StpDescendingComparer());
                _multipleSchedules.Add((_schedule.StpIndicator, _schedule.Calendar), _schedule);
                _schedule = null;
                _stateType = Multiplicity.MultipleSchedules;
            }
        }

        public ResolvedService GetScheduleOn(DateTime date)
        {
            
            if (_schedule != null)
            {
                if (_schedule.RunsOn(date))
                    return new ResolvedService(_schedule, date,_schedule.IsCancelled());

                return null;
            }

            var isCancelled = false;
            foreach (var schedule in _multipleSchedules.Values)
            {
                if (schedule.RunsOn(date))
                {
                    if (schedule.IsCancelled())
                        isCancelled = true;
                    else
                        return new ResolvedService(schedule, date, isCancelled);
                }
            }
            return null;
        }

        public bool TryGetScheduleOn(DateTime date, out ResolvedService schedule)
        {
            schedule = GetScheduleOn(date);
            return schedule != null;
        }
        
        public override string ToString()
        {
            return TimetableUid;
        }
    }
}