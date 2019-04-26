using System;
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

    /// <summary>
    /// Short Term Plan (STP) 
    /// </summary>
    /// <remarks>Order is by priority</remarks>
    public enum StpIndicator
    {
        Permanent = 0, // P - Permanent schedule
        Override = 1, // O - STP overlay of Permanent schedule
        New = 2, // N - New STP schedule (not an overlay)
        Cancelled = 3, // C - STP Cancellation of Permanent schedule
    }

    /// <summary>
    /// An indivdual schedule
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// Unique internal id
        /// </summary>
        public int Id { get; }

        public Service Parent { get; private set;  }

        public string TimetableUid { get; set; }

        public StpIndicator StpIndicator { get; set; }

        public ICalendar Calendar { get; set; }

        public string RetailServiceId { get; set; }

        public Toc Operator { get; set; }

        public AccomodationClass SeatClass { get; set; }

        public AccomodationClass SleeperClass { get; set; }

        public ReservationIndicator ReservationIndicator { get; set; }

        /// <summary>
        /// Status - values incorporates transport mode and whether its permanent or STP
        /// </summary>
        /// <remarks>For values: https://wiki.openraildata.com/index.php?title=CIF_Codes#Train_Status </remarks>
        public string Status { get; set; }

        /// <summary>
        /// Train Category
        /// </summary>
        /// <remarks>For values: https://wiki.openraildata.com/index.php?title=CIF_Codes#Train_Category </remarks>
        public string Category { get; set; }

        public Schedule(int id)
        {
            Id = id;
        }

        public void AddToService(Service service)
        {
            service.Add(this);            
            Parent = service;
        }

        public IReadOnlyList<ScheduleLocation> Locations => _locations;
        
        private List<ScheduleLocation> _locations = new List<ScheduleLocation>(8);

        internal void AddLocation(ScheduleLocation location) => _locations.Add(location);
        
        public bool RunsOn(DateTime date)
        {
            return Calendar.IsActiveOn(date);
        }

        public override string ToString()
        {
            return $"{TimetableUid} -{StpIndicator} {Calendar} ({Id})";
        }
    }
}