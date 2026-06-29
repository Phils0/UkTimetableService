using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{ 
    public interface IPublicSchedule
    {
        void AddService(IServiceTime serviceTime);
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

        // The slot key stays day-ignoring (ordering relies on it), but each entry keeps its own stop time so
        // the gatherer doesn't substitute the shared key for every service that shares the slot.
        private readonly SortedList<Time, IServiceTime[]> _serviceTimes;
        private readonly TimesToUse _arrivalsOrDepartures;
        private readonly IComparer<Time> _comparer;

        internal PublicSchedule(Station at, TimesToUse arrivalsOrDepartures, IComparer<Time> comparer)
        {
            Location = at;
            _arrivalsOrDepartures = arrivalsOrDepartures;
            _comparer = comparer;
            _serviceTimes = new SortedList<Time, IServiceTime[]>(comparer);
        }

        public void AddService(IServiceTime serviceTime)
        {
            var time = serviceTime.Time;
            if (!_serviceTimes.ContainsKey(time))
                _serviceTimes.Add(time, new[] {serviceTime});
            else
            {
                var existing = _serviceTimes[time];
                // Dedup on service AND exact time: a service's own 00:xx and 24:xx are different stops in one
                // day-ignoring slot and must both survive; only a true (same service, same time) repeat is dropped.
                if (!existing.Any(s => s.Service.Equals(serviceTime.Service) && s.Time.Equals(serviceTime.Time)))
                {
                    var length = existing.Length;
                    Array.Resize(ref existing, length + 1);
                    existing[length] = serviceTime;
                    _serviceTimes[time] = existing;
                }
            }
        }

        public (Time time, IServiceTime[] serviceTimes) ValuesAt(int index)
        {
            return (_serviceTimes.Keys[index], _serviceTimes.Values[index]);
        }

        public IEnumerable<KeyValuePair<Time, IServiceTime[]>> GetValuesBefore(int index) => _serviceTimes.Take(index);
        public IEnumerable<KeyValuePair<Time, IServiceTime[]>> GetValuesAtAndAfter(int index) => _serviceTimes.Skip(index);
       
        public int Count => _serviceTimes.Count;

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
            return time.Equals(_serviceTimes.Keys[idx]);
        }

        private (int index, bool changeDay) FindNearestTime(Time time)
        {
            for (int i = 0; i < _serviceTimes.Count; i++)
            {
                if (_comparer.Compare(_serviceTimes.Keys[i], time) >= 0) //  Point when changes from being less to more
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