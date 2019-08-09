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
        /// <param name="config">Configuration for gathering the results</param>
        /// <returns>Schedules of running services.  If a service departs at time counts as first of after.</returns>
        ResolvedServiceStop[] FindDepartures(DateTime at, GatherConfiguration config);

        /// <summary>
        /// Scheduled services arriving on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="config">Configuration for gathering the results</param>
        /// <returns>Schedules of running services.  If a service arrives at time counts as first of before.</returns>
        ResolvedServiceStop[] FindArrivals(DateTime at, GatherConfiguration config);
    }

    public class LocationTimetable : ILocationTimetable
    {
        private readonly PublicSchedule _arrivals;
        private readonly PublicSchedule _departures;


        public LocationTimetable(Station at)
        {
            _arrivals = new PublicSchedule(at, Time.EarlierLaterComparer);
            _departures = new PublicSchedule(at, Time.EarlierLaterComparer);
            
        }

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
        public ResolvedServiceStop[] FindDepartures(DateTime at, GatherConfiguration config)
        {
            return _departures.FindServices(at, config);
        }
        
        /// <summary>
        /// Scheduled services arriving on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return.  First found after or on time is always included in after</param>
        /// <returns>Schedules of running services.</returns>
        public ResolvedServiceStop[] FindArrivals(DateTime at, GatherConfiguration config)
        {
            return _arrivals.FindServices(at, config);
        }
    }
}