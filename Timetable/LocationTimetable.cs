using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Timetable
{
    public interface ILocationTimetable
    {
        /// <summary>
        /// Scheduled services departing on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return</param>
        /// <returns>Schedules of running services.  If a service departs at time counts as first of after.</returns>
        ResolvedService[] FindDepartures(DateTime at, int before, int after);

        /// <summary>
        /// Scheduled services arriving on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return</param>
        /// <returns>Schedules of running services.  If a service arrives at time counts as first of before.</returns>
        ResolvedService[] FindArrivals(DateTime at, int before, int after);
    }

    public class LocationTimetable : ILocationTimetable
    {
        private readonly PublicSchedule _arrivals = new PublicSchedule(Time.EarlierLaterComparer);
        private readonly PublicSchedule _departures = new PublicSchedule(Time.EarlierLaterComparer);

        internal void AddService(ScheduleLocation stop)
        {
            if (stop is IArrival arrival && arrival.IsPublic)
                _arrivals.AddService(new ArrivalServiceTime(arrival));

            if (stop is IDeparture departure && departure.IsPublic)
                _departures.AddService(new DepartureServiceTime(departure));
        }

        /// <summary>
        /// Scheduled services departing on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return.  First found after or on time is always included in after</param>
        /// <returns>Schedules of running services.</returns>
        public ResolvedService[] FindDepartures(DateTime at, int before, int after)
        {
            return _departures.FindServices(at, before, after);
        }
        
        /// <summary>
        /// Scheduled services arriving on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return.  First found after or on time is always included in after</param>
        /// <returns>Schedules of running services.</returns>
        public ResolvedService[] FindArrivals(DateTime at, int before, int after)
        {
            return _arrivals.FindServices(at, before, after);
        }
    }
}