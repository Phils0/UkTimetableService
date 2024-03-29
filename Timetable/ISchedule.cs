﻿using System;
using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// Accomodation classes supported
    /// </summary>
    public enum AccomodationClass
    {
        None, // Not available (Sleepers only)
        Both, // B Both First and Standard
        Standard, // S Standard only
        First // F First only (Sleepers only)
    }

    /// <summary>
    /// Possible reservation settings, making the ARSE mnemonic
    /// </summary>
    public enum ReservationIndicator
    {
        None, // Not supported
        Mandatory, // A Always - Manadatory
        Recommended, // R Recommended
        Supported, // S Supported
        EssentialBikes // E Essential for bicycles - never seen this value set
    }
    
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
        /// 2 char\6 digit Retail Service ID - used by NRS
        /// </summary>
        /// <remarks>
        /// First 4 digits indicate the logical service
        /// with last 2 digits for splits and joins
        /// </remarks>
        string RetailServiceId { get; }
        /// <summary>
        /// 2 char\4 digit Retail Service ID - used by NRS
        /// </summary>
        /// <remarks>
        /// Indicates the retail service in NRS
        /// Use to not have to worry about splits and joins
        /// </remarks>
        string NrsRetailServiceId { get; }
        /// <summary>
        /// Train Identity - sometimes called HeadCode
        /// </summary>
        string TrainIdentity { get; }
        /// <summary>
        /// Toc
        /// </summary>
        Toc Operator { get; }
        /// <summary>
        /// Seat Accomodation class
        /// </summary>
        AccomodationClass SeatClass { get; }
        /// <summary>
        /// Sleeper Accomodation Class
        /// </summary>
        AccomodationClass SleeperClass { get; }
        /// <summary>
        ///  Reservation indicator
        /// </summary>
        ReservationIndicator ReservationIndicator { get; }
        /// <summary>
        /// Status - values incorporates transport mode and whether its permanent or STP
        /// </summary>
        /// <remarks>For values: https://wiki.openraildata.com/index.php?title=CIF_Codes#Train_Status </remarks>
        string Status { get; }
        /// <summary>
        /// Train Category
        /// </summary>
        /// <remarks>For values: https://wiki.openraildata.com/index.php?title=CIF_Codes#Train_Category </remarks>
        string Category { get; }
        /// <summary>
        /// Stops and passing points
        /// </summary>
        IReadOnlyList<ScheduleLocation> Locations { get; }
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
        /// Schedule has the retail service id
        /// </summary>
        /// <param name="retailServiceId"></param>
        /// <returns></returns>
        bool HasRetailServiceId(string retailServiceId);
        /// <summary>
        /// Is Operating Toc
        /// </summary>
        /// <param name="toc"></param>
        /// <returns></returns>
        bool IsOperatedBy(string toc);
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
        /// Is public schedule
        /// </summary>
        /// <returns></returns>
        bool IsPublicSchedule();
    }
}