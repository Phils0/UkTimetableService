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

        internal ResolvedServiceStop[] FindServices(DateTime at, GatherConfiguration config)
        {
            var on = at.Date;
            var time = new Time(at.TimeOfDay);

            var first = FindNearestTime(time);
            
            if (config.HasRequestedBeforeOnly && EqualsTime(first.index, time))
                first.index = first.index + 1; 
            
            //TODO if need to change day do we go forward or backward
            
            return GatherServices(first.index, @on, config);
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
        
        private ResolvedServiceStop[] GatherServices(int startIdx, DateTime @on, GatherConfiguration config)
        {
            var gatherer = new ScheduleGatherer(this, config);
            return gatherer.Gather(startIdx, @on);
        }
    }
}