using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    public class CifService : Service
    {
        private readonly ILogger _logger;
        
        public string TimetableUid { get; }

        private CifSchedule _schedule;

        private SortedList<(StpIndicator indicator, ICalendar calendar), CifSchedule> _multipleSchedules;
        
        private AssociationDictionary _associations;

        public CifService(string timetableUid, ILogger logger)
        {
            _logger = logger;
            TimetableUid = timetableUid;
        }

        // Used by Schedule, do not use.  Use Schedule.AddToService
        internal void Add(CifSchedule schedule)
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
                    new SortedList<(StpIndicator indicator, ICalendar calendar), CifSchedule>(StpDescendingComparer.Instance);
                _multipleSchedules.Add((_schedule.StpIndicator, _schedule.Calendar), _schedule);
                _schedule = null;
            }
        }

        private bool HasNoState => _schedule == null && _multipleSchedules == null;
        private bool HasSingleSchedule => _schedule != null;
        
        public bool TryResolveOn(DateTime date, out ResolvedService schedule, bool resolveAssociations = true)
        {
            schedule = GetScheduleOn(date, resolveAssociations);
            return schedule != null;
        }
        
        private static readonly ResolvedAssociation[] None = new ResolvedAssociation[0];
        
        private ResolvedService GetScheduleOn(DateTime date, bool resolveAssociations)
        {
            ResolvedService CreateResolvedService(CifSchedule schedule, bool cancelled)
            {
                var resolvedAssociations = HasAssociations() && resolveAssociations
                    ? _associations.Resolve(schedule.TimetableUid, date, schedule.NrsRetailServiceId)
                    : None;
                
                return resolvedAssociations.Any() ?
                    new ResolvedServiceWithAssociations(schedule, date, cancelled, resolvedAssociations) :
                    new ResolvedService(schedule, date, cancelled);
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
            if (TryResolveOn(find.OnDate, out var schedule))
                return schedule.TryFindStop(find, out stop);

            stop = null;
            return false;
        }

        public void AddAssociation(Association association, bool isMain)
        {
            if (!HasAssociations())
                _associations = new AssociationDictionary(1, _logger);

            if(_associations.Add(association, isMain, this))
                association.SetService(this, isMain);
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

        internal bool TryGetSchedule((StpIndicator indicator, ICalendar calendar) key, out CifSchedule schedule)
        {
            schedule = null;
            if (HasNoState)
                return false;

            if (HasSingleSchedule)
            {
                // Assume its good
                schedule = _schedule;
                return true;
            }

            // Simple match, may sometimes not find a match when should
            return _multipleSchedules.TryGetValue(key, out schedule);
        }
        
        public override string ToString()
        {
            return HasAssociations() ? $"{TimetableUid} +{_associations.Count}" : TimetableUid;
        }
    }
}