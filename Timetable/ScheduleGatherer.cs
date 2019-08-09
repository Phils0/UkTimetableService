using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    internal class ScheduleGatherer
    {
        private readonly ISchedule _schedule;
        internal ScheduleGatherer(ISchedule schedule)
        {
            _schedule = schedule;
        }

        /// <summary>
        /// Gathers a set of 
        /// </summary>
        /// <param name="startIdx"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="on"></param>
        /// <returns></returns>
        internal ResolvedServiceStop[] Gather(int startIdx, int before, int after, DateTime @on)
        {
            var beforeServices = SelectServicesBefore(startIdx - 1, before, @on).Reverse();
            var afterServices = SelectServicesAfter(startIdx, after, on);
            return beforeServices.Concat(afterServices).ToArray();
        }

        private IEnumerable<ResolvedServiceStop>  SelectServicesBefore(int startIdx, int quantity, DateTime date)
        {
            return quantity == 0 || startIdx < 0? 
                Enumerable.Empty<ResolvedServiceStop>() :
                IterateBackwardsThroughServices();

            IEnumerable<ResolvedServiceStop> IterateBackwardsThroughServices()
            {
                int found = 0;
                int idx = startIdx;
                while (found < quantity && idx >= 0)
                {
                    var pair = _schedule.ValuesAt(idx);
                    foreach (var stop in CheckServicesForIndex(pair.services, pair.time, date))
                    {
                        found++;
                        yield return stop;
                    }

                    idx--;
                }
            }
        }
        
        private IEnumerable<ResolvedServiceStop> CheckServicesForIndex(Service[] services, Time atTime, DateTime date)
        {
            foreach (var service in services)
            {
                if (service.TryFindScheduledStopOn(date, _schedule.Location, atTime, out var stop))
                    yield return stop;
            }
        }
        
        private IEnumerable<ResolvedServiceStop> SelectServicesAfter(int startIdx, int quantity, DateTime date)
        {
            int stopIterating = _schedule.Count;
            return quantity == 0 || startIdx >= stopIterating ? 
                Enumerable.Empty<ResolvedServiceStop>() :
                IterateForwardsThroughServices();

            IEnumerable<ResolvedServiceStop> IterateForwardsThroughServices()
            {
                int found = 0;
                int idx = startIdx;
                while (found < quantity && idx < stopIterating)
                {
                    var pair = _schedule.ValuesAt(idx);
                    foreach (var stop in CheckServicesForIndex(pair.services, pair.time, date))
                    {
                        found++;
                        yield return stop;
                    }

                    idx++;
                }
            }
        }
    }
}