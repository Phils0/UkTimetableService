using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    /// <summary>
    /// Represents the Public Arrivals or Departures for a location
    /// </summary>
    /// <remarks>Services are ordered by time</remarks>
    internal class PublicSchedule
    {
        private readonly SortedList<Time, Service[]> _services;
        private readonly IComparer<Time> _comparer;

        internal PublicSchedule(IComparer<Time> comparer)
        {
            _comparer = comparer;
            _services = new SortedList<Time, Service[]>(comparer);
        }

        internal void AddService(IServiceTime stop)
        {
            if (!stop.IsPublic)
                return;
            
            var time = stop.Time;
            if (!_services.ContainsKey(time))
                _services.Add(time, new[] {stop.Service});
            else
            {
                var services = _services[time];
                if (!services.Any(s => s.Equals(stop.Service)))
                {
                    var length = services.Length;
                    Array.Resize(ref services, length + 1);
                    services[length] = stop.Service;
                    _services[time] = services;
                }
            }
        }

        internal Service[] GetServices(Time time)
        {
            return _services.TryGetValue(time, out var services) ? services : new Service[0];
        }

        internal ResolvedService[] FindServices(DateTime at, int before, int after)
        {
            if (before == 0 && after == 0)
                after = 1;
            
            var on = at.Date;
            var time = new Time(at.TimeOfDay);

            var first = FindStartIndex(time);

            // If find service at specific time and only returning before ensure we return the service at the time
            if (after == 0 && EqualsTime(first.index, time))
                first.index = first.index + 1; 
            
            if (first.changeDay) // Start on next day
                on = on.AddDays(1);
            return GetResults(first.index, before, after, on);
        }

        private bool EqualsTime(int idx, Time time)
        {
            return time.Equals(_services.Keys[idx]);
        }

        private (int index, bool changeDay) FindStartIndex(Time time)
        {
            for (int i = 0; i < _services.Count; i++)
            {
                if (_comparer.Compare(_services.Keys[i], time) >= 0) //  Point when changes from being less to more
                    return (i, false);
            }

            // Last one is still before so therefore first is next\previous day
            return (0, true);
        }

        private ResolvedService[] GetResults(int firstIdx, int requiredBack, int requiredForward, DateTime date)
        {
            var schedules = new List<ResolvedService>(requiredBack + requiredForward);

            bool checkedAll = false;
            int lastCheckedIdx = FindResultsGoingBackwards(requiredBack, firstIdx - 1);
            if (checkedAll) // Tried all, exit with what we found
                return schedules.ToArray();
            FindResultsGoingForward(requiredForward, firstIdx, lastCheckedIdx);

            return schedules.ToArray();

            int FindResultsGoingBackwards(int required, int startIdx)
            {
                int found = 0;
                int idx = startIdx;
                while (found < required && !checkedAll)
                {
                    if (idx < 0) // Previous loop checked first element, go to last
                        idx = _services.Count - 1;

                    var services = _services.Values[idx];
                    foreach (var service in _services.Values[idx])
                    {
                        if (service.TryGetScheduleOn(date, out var schedule))
                        {
                            schedules.Insert(0, schedule);
                            found++;
                        }
                    }

                    idx--;
                    checkedAll = (idx == startIdx); // Have we tried all
                }

                return ++idx; // Did not check last index
            }

            void FindResultsGoingForward(int required, int startIdx, int alreadyTriedIndex)
            {
                int found = 0;
                int idx = startIdx;
                while (found < required && !checkedAll)
                {
                    if (idx == _services.Count) // Last element  loop to first
                        idx = 0;

                    var services = _services.Values[idx];
                    foreach (var service in _services.Values[idx])
                    {
                        if (service.TryGetScheduleOn(date, out var schedule))
                        {
                            schedules.Add(schedule);
                            found++;
                        }
                    }

                    idx++;
                    checkedAll = (idx == alreadyTriedIndex || idx == startIdx); // We tried them all
                }
            }
        }
    }
}