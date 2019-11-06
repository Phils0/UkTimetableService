using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Serilog;

namespace Timetable
{
    public class Service
    {
        private readonly ILogger _logger;

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
        
        private Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> _associations;

        public Service(string timetableUid, ILogger logger)
        {
            _logger = logger;
            TimetableUid = timetableUid;
        }

        // Used by Schedule, do not use.  Use Schedule.AddToService
        internal void Add(Schedule schedule)
        {
            if (_stateType == Multiplicity.None)
            {
                SetSingleSchedule();
                return;
            }
            else if (_stateType == Multiplicity.SingleSchedule)
            {
                MoveToMultipleSchedules();
            }

            try
            {
                _multipleSchedules.Add((schedule.StpIndicator, schedule.Calendar), schedule);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Schedule already added {schedule}", e);
            }

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

        public bool TryFindScheduleOn(DateTime date, out ResolvedService schedule)
        {
            schedule = GetScheduleOn(date);
            return schedule != null;
        }
        
        public ResolvedService GetScheduleOn(DateTime date)
        {
            if (_schedule != null)
            {
                if (_schedule.RunsOn(date))
                    return new ResolvedService(_schedule, date, _schedule.IsCancelled(), GetAssociationsOn(date));

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
                        return new ResolvedService(schedule, date, isCancelled, GetAssociationsOn(date));
                }
            }

            return null;
        }

        private ResolvedAssociation[] GetAssociationsOn(DateTime date)
        {
            if(_associations == null)
                return new ResolvedAssociation[0];

            var associations = new List<ResolvedAssociation>();
            foreach (var versions in _associations.Values)
            {
                var isCancelled = false;
                foreach (var association in versions.Values)
                {
                    if (association.AppliesOn(date))
                    {
                        if (association.IsCancelled())
                            isCancelled = true;
                        else
                            associations.Add(new ResolvedAssociation(association, date, isCancelled));
                    }
                }
            }

            return associations.ToArray();
        }

        public bool TryFindScheduledStop(StopSpecification find, out ResolvedServiceStop stop)
        {
            var found = TryFindStopOn(find, out stop);

            if (found && stop.IsNextDay(find.UseDeparture))
            {
                find = find.MoveToPreviousDay();
                return TryFindStopOn(find, out stop);
            }

            return found;
        }

        private bool TryFindStopOn(StopSpecification find, out ResolvedServiceStop stop)
        {
            if (TryFindScheduleOn(find.OnDate, out var schedule))
                return schedule.TryFindStop(find, out stop);

            stop = null;
            return false;
        }

        internal void AddAssociation(Association association, bool isMain)
        {
            if (_associations == null)
                _associations = new Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>();

            if (isMain)
                Add(association.AssociatedTimetableUid);
            else
                Add(association.MainTimetableUid);

            void Add(string uid)
            {
                if (!_associations.TryGetValue(uid, out var values))
                {
                    values = new SortedList<(StpIndicator indicator, ICalendar calendar), Association>(new StpDescendingComparer());
                    _associations.Add(uid, values);
                }

                try
                {
                    values.Add((association.StpIndicator, association.Calendar), association);
                }
                catch (ArgumentException e)
                {
                    if(values.TryGetValue((association.StpIndicator, association.Calendar), out var duplicate))
                    {
                        // Can have 2 associations that are different but for the same services, see tests
                        _logger.Warning("Removing Duplicate Associations {association} {duplicate}", association, duplicate);
                        RemoveAssociation();
                    }
                    else
                        throw;
                    
                    void RemoveAssociation()
                    {
                        values.Remove((association.StpIndicator, association.Calendar));
                        if (!values.Any())
                            _associations.Remove(uid);
                    }                    
                }


            }
        }


        public bool HasAssociations()
        {
            return _associations != null && _associations.Any();
        }
        
        public override string ToString()
        {
            return HasAssociations() ? $"{TimetableUid} +{_associations.Count}" : TimetableUid;
        }
    }
}