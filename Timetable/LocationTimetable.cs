using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Timetable
{
    public class LocationTimetable
    {
        private readonly SortedList<Time, Service[]> _arrivals =
            new SortedList<Time, Service[]>(256, Time.LaterEarlierComparer);

        private readonly SortedList<Time, Service[]> _departures =
            new SortedList<Time, Service[]>(256, Time.EarlierLaterComparer);

        internal void AddService(ScheduleLocation stop)
        {
            if (stop is IArrival arrival)
            {
                var time = arrival.GetTime();
                if (!_arrivals.ContainsKey(time))
                    _arrivals.Add(time, new[] {arrival.Service});
                else
                    AddSchedule(time, arrival.Service, _arrivals);
            }

            if (stop is IDeparture departure)
            {
                var time = departure.GetTime();
                if (!_departures.TryGetValue(time, out var services))
                    _departures.Add(time, new[] {departure.Service});
                else
                    AddSchedule(time, departure.Service, _departures);
            }

            void AddSchedule(Time time, Service service, SortedList<Time, Service[]> appendTo)
            {
                var services = appendTo[time];
                if (!services.Contains(service))
                {
                    var length = services.Length;
                    Array.Resize(ref services, length + 1);
                    services[length] = service;
                    appendTo[time] = services;
                }
            }
        }

        /// <summary>
        /// Gets services for a particular departure time 
        /// </summary>
        /// <param name="time">specific time of departure</param>
        /// <returns>Possible Services, note does not check if they are running as has no date</returns>
        public Service[] GetDepartures(Time time)
        {
            return _departures.TryGetValue(time, out var services) ? services : new Service[0];
        }

        /// <summary>
        /// Gets services for a particular arrival time 
        /// </summary>
        /// <param name="time">specific arrival time</param>
        /// <returns>Possible Services, note does not check if they are running as has no date</returns>
        public Service[] GetArrivals(Time time)
        {
            return _arrivals.TryGetValue(time, out var services) ? services : new Service[0];
        }

        /// <summary>
        /// Scheduled services departing on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return</param>
        /// <returns>Schedules of running services.  If a service departs at time counts as first of after.</returns>
        public Schedule[] FindDepartures(DateTime at, int before = 1, int after = 5)
        {
            var on = at.Date;
            var time = new Time(at.TimeOfDay);

            var first = FindStartIndex(time, _departures);
            if (first.changeDay)    // Start on next day
                on = on.AddDays(1);
            return GetResults(first.index, _departures, before, after, on);
        }

        private (int index, bool changeDay) FindStartIndex(Time time, SortedList<Time, Service[]> timetable)
        {
            var comparer = timetable.Comparer;

            for (int i = 0; i < timetable.Count; i++)
            {
                if (comparer.Compare(timetable.Keys[i], time) >= 0)     //  Point when changes from being less to more
                    return (i, false);
            }

            // Last one is still before so therefore first is next\previous day
            return (0, true);
        }

        private Schedule[] GetResults(int firstIdx, SortedList<Time, Service[]> timetable, int requiredBack, int requiredForward, DateTime date)
        {
            var schedules = new Dictionary<string, Schedule>(requiredBack + requiredForward);

            bool checkedAll = false;           
            int lastCheckedIdx = FindResultsGoingBackwards(requiredBack, firstIdx - 1);
            if (checkedAll) // Tried all, exit with what we found
                return schedules.Values.ToArray();
            FindResultsGoingForward(requiredForward, firstIdx, lastCheckedIdx);

            return schedules.Values.ToArray();

            int FindResultsGoingBackwards(int required, int startIdx)
            {
                int found = 0;
                int idx = startIdx;
                while (found < required && !checkedAll)
                {
                    if (idx < 0) // Previous loop checked first element, go to last
                        idx = timetable.Count - 1;

                    var services = timetable.Values[idx];
                    foreach (var service in timetable.Values[idx])
                    {
                        if (service.TryGetScheduleOn(date, out var schedule))
                        {
                            schedules.Add(service.TimetableUid, schedule);
                            found++;
                        }
                    }

                    idx--;
                    checkedAll = (idx == startIdx); // Have we tried all
                }

                return ++idx;    // Did not check last index
            }

            void FindResultsGoingForward(int required, int startIdx, int alreadyTriedIndex)
            {
                int found = 0;
                int idx = startIdx;
                while (found < required && !checkedAll)
                {
                    if (idx == timetable.Count) // Last element  loop to first
                        idx = 0;

                    var services = timetable.Values[idx];
                    foreach (var service in timetable.Values[idx])
                    {
                        if (service.TryGetScheduleOn(date, out var schedule))
                        {
                            schedules.Add(service.TimetableUid, schedule);
                            found++;
                        }
                    }

                    idx++;
                    checkedAll = (idx == alreadyTriedIndex || idx == startIdx); // We tried them all
                }
            }
        }

        /// <summary>
        /// Scheduled services arriving on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return</param>
        /// <returns>Schedules of running services.  If a service arrives at time counts as first of before.</returns>
        public Schedule[] FindArrivals(DateTime at, int before = 5, int after = 1)
        {
            var on = at.Date;
            var time = new Time(at.TimeOfDay);

            var first = FindStartIndex(time, _arrivals);
            if (first.changeDay)  // Start on previous day
                on = on.AddDays(-1);
            return GetResults(first.index, _arrivals, after, before, on);
        }
    }
}