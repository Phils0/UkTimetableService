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
        private sealed class StpDescendingComparer : IComparer<(StpIndicator indicator, ICalendar calendar)>
        {
            public int Compare((StpIndicator indicator, ICalendar calendar) x,
                (StpIndicator indicator, ICalendar calendar) y)
            {
                var compare = y.indicator.CompareTo(x.indicator);
                return compare != 0 ? compare : x.calendar.CompareTo(y.calendar);
            }
        }
        
        private readonly ILogger _logger;
        
        public string TimetableUid { get; }

        private Schedule _schedule;

        private SortedList<(StpIndicator indicator, ICalendar calendar), Schedule> _multipleSchedules;
        
        private Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> _associations;

        public Service(string timetableUid, ILogger logger)
        {
            _logger = logger;
            TimetableUid = timetableUid;
        }

        // Used by Schedule, do not use.  Use Schedule.AddToService
        internal void Add(Schedule schedule)
        {
            if(HasNoState)
            {
                _schedule = schedule;
                return;
            }
            
            if(HasSingleSchedule)
                MoveToMultipleSchedules();

            try
            {
                _multipleSchedules.Add((schedule.StpIndicator, schedule.Calendar), schedule);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Schedule already added {schedule}", e);
            }
            
            void MoveToMultipleSchedules()
            {
                _multipleSchedules =
                    new SortedList<(StpIndicator indicator, ICalendar calendar), Schedule>(new StpDescendingComparer());
                _multipleSchedules.Add((_schedule.StpIndicator, _schedule.Calendar), _schedule);
                _schedule = null;
            }
        }

        private bool HasNoState => _schedule == null && _multipleSchedules == null;
        private bool HasSingleSchedule => _schedule != null;
        
        public bool TryFindScheduleOn(DateTime date, out ResolvedService schedule)
        {
            schedule = GetScheduleOn(date);
            return schedule != null;
        }
        
        public ResolvedService GetScheduleOn(DateTime date, bool resolveAssociations = true)
        {
            ResolvedService CreateResolvedService(Schedule schedule, bool cancelled)
            {
                return HasAssociations() && resolveAssociations
                    ? new ResolvedServiceWithAssociations(schedule, date, cancelled, _associations)
                    : new ResolvedService(schedule, date, cancelled);
            }
            
            if(HasSingleSchedule)
                return _schedule.RunsOn(date) ? CreateResolvedService(_schedule, _schedule.IsCancelled()) : null;

            var isCancelled = false;
            foreach (var schedule in _multipleSchedules.Values)
            {
                if (schedule.RunsOn(date))
                {
                    if (schedule.IsCancelled())
                        isCancelled = true;
                    else
                        return CreateResolvedService(schedule, isCancelled);
                }
            }

            return null;
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
            if (!HasAssociations())
                _associations = new Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>(1);

            if (isMain)
            {
                Add(association.Associated.TimetableUid);
                association.SetService(this, true);
            }
            else
            {
                Add(association.Main.TimetableUid);
                association.SetService(this, false);
            }

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

        public bool StartsBefore(Time time)
        {
            var schedule = HasSingleSchedule ? _schedule : _multipleSchedules.Values.First(s => !s.IsCancelled());
            
            var origin = schedule.Locations.First() as IDeparture;
            return origin.Time.IsBeforeIgnoringDay(time);
        }
        
        public override string ToString()
        {
            return HasAssociations() ? $"{TimetableUid} +{_associations.Count}" : TimetableUid;
        }
    }
}