using System;
using System.Collections.Generic;

namespace Timetable
{
    public interface ISchedule
    {
        /// <summary>
        /// Service container
        /// </summary>
        IService Service { get; }
        /// <summary>
        /// Add this schedule to the service
        /// </summary>
        /// <param name="service"></param>
        void AddToService(IService service);
        /// <summary>
        /// Timetable Id
        /// </summary>
        string TimetableUid { get; }
        /// <summary>
        /// When schedule runs
        /// </summary>
        ICalendar Calendar { get; }
        /// <summary>
        /// Stops and passing points
        /// </summary>
        IReadOnlyList<ScheduleLocation> Locations { get; }
        /// <summary>
        /// Toc
        /// </summary>
        Toc Operator { get; }
        
        /// <summary>
        /// Status - values incorporates transport mode and whether its permanent or STP
        /// </summary>
        /// <remarks>For values: https://wiki.openraildata.com/index.php?title=CIF_Codes#Train_Status </remarks>
        string Status { get; }
        /// <summary>
        /// Service Characteristics
        /// </summary>
        IScheduleProperties Properties { get; }
        /// <summary>
        /// 2 char\4 digit Retail Service ID
        /// </summary>
        /// <remarks>
        /// Indicates the retail service
        /// Use to not have to worry about splits and joins
        /// </remarks>
        string ShortRetailServiceId { get; }
        /// <summary>
        /// Public arrivals
        /// </summary>
        IEnumerable<IArrival> Arrivals { get; }
        /// <summary>
        /// Public Departures
        /// </summary>
        IEnumerable<IDeparture> Departures { get; }
        /// <summary>
        /// Schedule Origin
        /// </summary>
        ScheduleStop Origin { get; }
        /// <summary>
        /// Schedule Terminus
        /// </summary>
        ScheduleStop Destination { get; }
        /// <summary>
        /// Schedule cancelled
        /// </summary>
        /// <returns></returns>
        bool IsCancelled();
        /// <summary>
        /// Schedule runs on date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        bool RunsOn(DateTime date);
        /// <summary>
        /// Try find stop
        /// </summary>
        /// <param name="find"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        bool TryFindStop(StopSpecification find, out ScheduleLocation stop);
        /// <summary>
        /// Get a specific stop
        /// </summary>
        /// <param name="at"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        ScheduleLocation GetStop(Location at, int sequence);
        /// <summary>
        /// Is Operating Toc
        /// </summary>
        /// <param name="toc"></param>
        /// <returns></returns>
        bool IsOperatedBy(string toc);
    }
}