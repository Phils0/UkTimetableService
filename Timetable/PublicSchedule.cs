using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public interface ISchedule
    {
        Station Location { get; }
        (Time time, Service[] services) ValuesAt(int index);
        int Count { get; }
    }

    /// <summary>
    /// Represents the Public Arrivals or Departures for a location
    /// </summary>
    /// <remarks>Services are ordered by time</remarks>
    internal class PublicSchedule : ISchedule
    {
        public Station Location { get; }

        private readonly SortedList<Time, Service[]> _services;
        private readonly IComparer<Time> _comparer;

        internal PublicSchedule(Station at, IComparer<Time> comparer)
        {
            Location = at;
            _comparer = comparer;
            _services = new SortedList<Time, Service[]>(comparer);
        }

        internal void AddService(IServiceTime stop)
        {
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

        public (Time time, Service[] services) ValuesAt(int index)
        {
            return (_services.Keys[index], _services.Values[index]);
        }

        public int Count => _services.Count;

        internal ResolvedServiceStop[] FindServices(DateTime at, int before, int after)
        {
            // Ensure return at least one service
            if (before == 0 && after == 0)
                after = 1;
            
            var on = at.Date;
            var time = new Time(at.TimeOfDay);

            var first = FindNearestTime(time);

            // If find service at specific time and only returning before ensure we return the service at the time
            if (after == 0 && EqualsTime(first.index, time))
                first.index = first.index + 1; 
            
            //TODO if need to change day do we go forward or backward
            
            return GatherServices(first.index, before, after, @on);
        }
        
        private bool EqualsTime(int idx, Time time)
        {
            return time.Equals(_services.Keys[idx]);
        }

        private (int index, bool changeDay) FindNearestTime(Time time)
        {
            for (int i = 0; i < _services.Count; i++)
            {
                if (_comparer.Compare(_services.Keys[i], time) >= 0) //  Point when changes from being less to more
                    return (i, false);
            }

            // Last one is still before so therefore first is next day
            return (0, true);
        }
        
        private ResolvedServiceStop[] GatherServices(int startIdx, int before, int after, DateTime @on)
        {
            var gatherer = new ScheduleGatherer(this);
            return gatherer.Gather(startIdx, before, after, @on);
        }
    }
}