using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public interface IGathererScheduleData
    {
        Station Location { get; }
        (Time time, IServiceTime[] serviceTimes) ValuesAt(int index);
        int Count { get; }
        IEnumerable<KeyValuePair<Time, IServiceTime[]>> GetValuesBefore(int index);
        IEnumerable<KeyValuePair<Time, IServiceTime[]>> GetValuesAtAndAfter(int index);
    }
    
    internal class ScheduleGatherer
    {
        private const int DayLimit = 3;
        
        private readonly IGathererScheduleData _schedule;
        private readonly GatherConfiguration _config;
        private readonly TimesToUse _arrivalsOrDestinations;

        private GatherConfiguration.GatherFilter FilterFoundServices => _config.Filter;
        
        internal ScheduleGatherer(IGathererScheduleData schedule, GatherConfiguration config, TimesToUse arrivalsOrDestinations)
        {
            _schedule = schedule;
            _config = config;
            _arrivalsOrDestinations = arrivalsOrDestinations;
        }
        
        /// <summary>
        /// Gathers a set of services
        /// </summary>
        /// <param name="startIdx">Start position in the data set</param>
        /// <param name="before">Number of services before</param>
        /// <param name="after">Number of services after, includes start position</param>
        /// <param name="onDate">Date </param>
        /// <returns>Array of services running on day</returns>
        internal ResolvedServiceStop[] Gather(int startIdx, DateTime onDate)
        {
            if (_config.All)
                return SelectAllServices(startIdx, onDate).ToArray();

            // Check of need to go back a day
            int beforeStartIdx = startIdx == 0 ? LastIdx : startIdx - 1;
            DateTime beforeDate = startIdx == 0 ? onDate.AddDays(-1) : onDate;
            
            var beforeServices = SelectServicesBefore(beforeStartIdx,  _config.ServicesBefore, beforeDate).Reverse();
            var afterServices = SelectServicesAfter(startIdx, _config.ServicesAfter,  onDate);
            return beforeServices.Concat(afterServices).ToArray();
        }
        
        private IEnumerable<ResolvedServiceStop>  SelectServicesBefore(int startIdx, int quantity, DateTime onDate)
        {
            return DoNotIterate() ? 
                Enumerable.Empty<ResolvedServiceStop>() :
                IterateBackwardsThroughServices();

            bool DoNotIterate()
            {
                return quantity == 0 || startIdx < 0;
            }

            IEnumerable<ResolvedServiceStop> IterateBackwardsThroughServices()
            {
                int found = 0;
                int idx = startIdx;
                DateTime resolveDate = onDate;
                bool keepSearching = true;
                int dayGuard = 0;
                
                while (keepSearching && (dayGuard < DayLimit))
                {
                    while (keepSearching && idx >= 0)
                    {
                        var pair = _schedule.ValuesAt(idx);
                        foreach (var stop in ResolveServices(pair.serviceTimes, resolveDate))
                        {
                            found++;
                            keepSearching = (found < quantity);
                            yield return stop;
                        }
                        idx--;
                    }

                    if (keepSearching)
                    {
                        resolveDate = resolveDate.AddDays(-1);
                        idx = LastIdx;
                        dayGuard++;
                    }
                }
            }
        }

        private int LastIdx => _schedule.Count - 1;

        private IEnumerable<ResolvedServiceStop> ResolveServices(IServiceTime[] serviceTimes, DateTime onDate)
        {
            var source = MatchServiceStop();
            return FilterFoundServices(source);

            IEnumerable<ResolvedServiceStop> MatchServiceStop()
            {
                foreach (var serviceTime in serviceTimes)
                {
                    // Use each entry's own time, not the shared slot key: a same-day 00:xx and a next-day
                    // 24:xx collapse into one day-ignoring slot, so the key may belong to a different service.
                    var find = new StopSpecification(_schedule.Location, serviceTime.Time, onDate, _arrivalsOrDestinations);
                    if (serviceTime.Service.TryFindScheduledStop(find, out var resolved))
                        yield return resolved;
                }
            }
        }
        
        private IEnumerable<ResolvedServiceStop> SelectServicesAfter(int startIdx, int quantity, DateTime onDate)
        {
            return DoNotIterate() ? 
                Enumerable.Empty<ResolvedServiceStop>() :
                IterateForwardsThroughServices();

            bool DoNotIterate()
            {
                return quantity == 0 || startIdx >= _schedule.Count;
            }

            IEnumerable<ResolvedServiceStop> IterateForwardsThroughServices()
            {
                int found = 0;
                int idx = startIdx;
                DateTime resolveDate = onDate;
                bool keepSearching = true;
                int dayGuard = 0;
                
                while (keepSearching && (dayGuard < DayLimit))
                {
                    while (keepSearching && idx < _schedule.Count)
                    {
                        var pair = _schedule.ValuesAt(idx);
                        foreach (var stop in ResolveServices(pair.serviceTimes, resolveDate))
                        {
                            found++;
                            keepSearching = (found < quantity);
                            yield return stop;
                        }
                        idx++;
                    }

                    if (keepSearching)
                    {
                        resolveDate = resolveDate.AddDays(1);
                        idx = 0;
                        dayGuard++;
                    }
                }
            }
        }
        
        private IEnumerable<ResolvedServiceStop> SelectAllServices(int startIdx, DateTime date)
        {
            foreach (var pair  in _schedule.GetValuesAtAndAfter(startIdx))
            {
                foreach (var stop in ResolveServices(pair.Value, date))
                    yield return stop;
            }

            var nextDay = date.AddDays(1);
            foreach (var pair in _schedule.GetValuesBefore(startIdx))
            {
                foreach (var stop in ResolveServices(pair.Value, nextDay))
                    yield return stop;
            }
        }
    }
}