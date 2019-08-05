using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Timetable
{
    public class LocationTimetable
    {
        private readonly PublicSchedule _arrivals = new PublicSchedule(Time.EarlierLaterComparer);
        private readonly PublicSchedule _departures = new PublicSchedule(Time.EarlierLaterComparer);

        internal void AddService(ScheduleLocation stop)
        {
            if (stop is IArrival arrival)
                _arrivals.AddService(new ArrivalServiceTime(arrival));

            if (stop is IDeparture departure)
                _departures.AddService(new DepartureServiceTime(departure));
        }

        /// <summary>
        /// Scheduled services departing on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return</param>
        /// <returns>Schedules of running services.  If a service departs at time counts as first of after.</returns>
        public ResolvedService[] FindDepartures(DateTime at, int before = 1, int after = 5)
        {
            return _departures.FindServices(at, before, after, IncludeFirst.InAfter);
        }
        
        /// <summary>
        /// Scheduled services arriving on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return</param>
        /// <returns>Schedules of running services.  If a service arrives at time counts as first of before.</returns>
        public ResolvedService[] FindArrivals(DateTime at, int before = 5, int after = 1)
        {
            return _arrivals.FindServices(at, before, after, IncludeFirst.InBefore);
        }
    }
}