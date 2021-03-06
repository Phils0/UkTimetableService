using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Timetable
{
    internal class LocationTimetable
    {
        private readonly IPublicSchedule _arrivals;
        private readonly IPublicSchedule _departures;
        
        public LocationTimetable(Station at) :
            this( 
                new PublicSchedule(at, TimesToUse.Arrivals, Time.EarlierLaterComparer), 
                new PublicSchedule(at, TimesToUse.Departures, Time.EarlierLaterComparer))
        {
        }
        
        public LocationTimetable(IPublicSchedule arrivals, IPublicSchedule departures)
        {
            _arrivals = arrivals;
            _departures = departures;
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
        /// Scheduled services departing on date
        /// </summary>
        /// <param name="onDate">Date and Time</param>
        /// <param name="filter">Any filter</param>
        /// <param name="dayBoundary"></param>
        /// <returns>Schedules of running services.</returns>
        public ResolvedServiceStop[] AllDepartures(DateTime onDate, GatherConfiguration.GatherFilter filter, Time dayBoundary)
        {
            return _departures.AllServices(onDate, filter, dayBoundary);
        }

        /// <summary>
        /// Scheduled services arriving on date near to time
        /// </summary>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return.  First found after or on time is always included in after</param>
        /// <param name="config"></param>
        /// <returns>Schedules of running services.</returns>
        public ResolvedServiceStop[] FindArrivals(DateTime at, GatherConfiguration config)
        {
            return _arrivals.FindServices(at, config);
        }

        /// <summary>
        /// Scheduled services arriving on date
        /// </summary>
        /// <param name="onDate">Date and Time</param>
        /// <param name="filter">Any filter</param>
        /// <param name="dayBoundary"></param>
        /// <returns>Schedules of running services.</returns>
        public ResolvedServiceStop[] AllArrivals(DateTime onDate, GatherConfiguration.GatherFilter filter,  Time dayBoundary)
        {
            return _arrivals.AllServices(onDate, filter, dayBoundary);
        }
    }
}