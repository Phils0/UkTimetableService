using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{ 
    public interface IPublicSchedule
    {
        void AddService(IServiceTime stop);
        ResolvedServiceStop[] FindServices(DateTime at, GatherConfiguration config);
        ResolvedServiceStop[] AllServices(DateTime onDate, GatherConfiguration.GatherFilter filter, Time dayBoundary);
    }

    /// <summary>
    /// Represents the Public Arrivals or Departures for a location
    /// </summary>
    /// <remarks>Services are ordered by time</remarks>
    internal class PublicSchedule : IPublicSchedule, IGathererScheduleData
    {
        public Station Location { get; }

        private readonly SortedList<Time, IService[]> _services;
        private readonly TimesToUse _arrivalsOrDepartures;
        private readonly IComparer<Time> _comparer;

        internal PublicSchedule(Station at, TimesToUse arrivalsOrDepartures, IComparer<Time> comparer)
        {
            Location = at;
            _arrivalsOrDepartures = arrivalsOrDepartures;
            _comparer = comparer;
            _services = new SortedList<Time, IService[]>(comparer);
        }

        public void AddService(IServiceTime stop)
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
        
        public (Time time, IService[] services) ValuesAt(int index)
        {
            return (_services.Keys[index], _services.Values[index]);
        }

        public IEnumerable<KeyValuePair<Time, IService[]>> GetValuesBefore(int index) => _services.Take(index);
        public IEnumerable<KeyValuePair<Time, IService[]>> GetValuesAtAndAfter(int index) => _services.Skip(index);
       
        public int Count => _services.Count;

        public ResolvedServiceStop[] FindServices(DateTime at, GatherConfiguration config)
        {
            var onDate = at.Date;
            var time = new Time(at.TimeOfDay);

            var first = FindNearestTime(time);
            
            if (config.HasRequestedBeforeOnly && EqualsTime(first.index, time))
                first.index += 1; 
            
            return GatherServices(first.index, onDate, config);
        }

        /// <summary>
        /// Gather all services
        /// </summary>
        /// <param name="onDate"></param>
        /// <param name="filter"></param>
        /// <param name="dayBoundary"></param>
        /// <returns>All services for day</returns>
        /// <remarks>Reuses standard Gather functionality, setting starting index to 0 and config to get all (actually lots) </remarks>
        public ResolvedServiceStop[] AllServices(DateTime onDate, GatherConfiguration.GatherFilter filter, Time dayBoundary)
        {
            var first = FindNearestTime(dayBoundary);
            
            var config = new GatherConfiguration(0, 0, true, filter);
            var gatherer = new ScheduleGatherer(this, config, _arrivalsOrDepartures);
            return gatherer.Gather(first.index, onDate.Date);
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
        
        private ResolvedServiceStop[] GatherServices(int startIdx, DateTime onDate, GatherConfiguration config)
        {
            var gatherer = new ScheduleGatherer(this, config, _arrivalsOrDepartures);
            return gatherer.Gather(startIdx, onDate);
        }
    }
}