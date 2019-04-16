using System;

namespace Timetable
{
    /// <summary>
    /// Accomodation classes supported
    /// </summary>
    public enum AccomodationClass
    {
        None,       // Not available (Sleepers only)
        Both,       // B Both First and Standard
        Standard,   // S Standard only
        First       // F First only (Sleepers only)
    }

    /// <summary>
    /// Possible reservation settings, making the ARSE mnemonic
    /// </summary>
    public enum ReservationIndicator
    {
        None,             // Not supported
        Mandatory,        // A Always - Manadatory
        Recommended,      // R Recommended
        Supported,        // S Supported
        EssentialBikes    // E Essential for bicycles - never seen this value set
    }

    /// <summary>
    /// Short Term Plan (STP) 
    /// </summary>
    public enum StpIndicator
    {
        Permanent = 0,    // P - Permanent schedule
        Override = 1,     // O - STP overlay of Permanent schedule
        New = 2,          // N - New STP schedule (not an overlay)
        Cancelled = 3,    // C - STP Cancellation of Permanent schedule
    }

    public static class ServiceStatus
    {
        public const string PermanentBus = "B";
        public const string PermanentFreight = "F";
        public const string PermanentPassenger = "P";
        public const string PermanentShip = "S";
        public const string PermanentTrip = "T";
        public const string StpPassenger = "1";
        public const string StpFreight = "2";
        public const string StpTrip = "3";
        public const string StpShip = "4";
        public const string StpBus = "5";
    }
    
    public static class ServiceCategory
    {
        public const string LondonUndergroundService = "OL";
        public const string UnadvertisedOrdinaryPassenger = "OU";
        public const string OrdinaryPassenger = "OO";
        public const string ChannelTunnel = "XC";
        public const string UnadvertisedExpress = "XU";
        public const string ExpressPassenger = "XX";
        public const string Sleeper = "XZ";
        public const string BusReplacement = "BR";
        public const string BusPermanent = "BS";
        public const string Ship = "SS";    // Not used?        
    }
        
    /// <summary>
    /// An indivdual schedule
    /// </summary>
    public class Schedule
    {
        public string TimetableUid { get; set; }
        
        public StpIndicator StpIndicator { get; set; }
        
        public ICalendar Calendar { get; set; }
        
        public string RetailServiceId { get; set; }
        
        public Toc Toc { get; set; }
        
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
        
        public IScheduleLocation[] Locations { get; set; }

        public bool RunsOn(DateTime date)
        {
            return Calendar.IsActiveOn(date);
        }
        
        public override string ToString()
        {
            return $"{TimetableUid} -{StpIndicator} {Calendar}";
        }
    }
}