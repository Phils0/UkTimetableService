using System;
using System.Collections.Generic;
using System.Linq;

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

        public Service Service { get; private set;  }
        /// <summary>
        /// Timetable Id
        /// </summary>
        public string TimetableUid { get; set; }

        /// <summary>
        /// STP (Short Term Plan) Indicator
        /// </summary>
        /// <remarks>
        /// P - Permanent schedule
        /// O - STP overlay of Permanent schedule
        /// N - New STP schedule (not an overlay)
        /// C - STP Cancellation of Permanent schedule
        /// </remarks>
        public StpIndicator StpIndicator { get; set; }

        public bool IsCancelled() => StpIndicator.Cancelled == StpIndicator;
        
        public ICalendar Calendar { get; set; }
        /// <summary>
        /// 2 char\6 digit Retail Service ID - used by NRS
        /// </summary>
        /// <remarks>
        /// First 4 digits indicate the logical service
        /// with last 2 digits for splits and joins
        /// </remarks>
        public string RetailServiceId { get; set; }

        /// <summary>
        /// 2 char\4 digit Retail Service ID - used by NRS
        /// </summary>
        /// <remarks>
        /// Indicates the retail service in NRS
        /// Use to not have to worry about splits and joins
        /// </remarks>
        public string ShortRetailServiceId
        {
            get { return string.IsNullOrEmpty(RetailServiceId) ? "" : RetailServiceId.Substring(0, 6); }
        }
        
        /// <summary>
        /// Train Identity - sometimes called HeadCode
        /// </summary>
        public string TrainIdentity { get; set; }

        /// <summary>
        /// Toc
        /// </summary>
        public Toc Operator { get; set; }
        /// <summary>
        /// Seat Accomodation class
        /// </summary>
        public AccomodationClass SeatClass { get; set; }
        /// <summary>
        /// Sleeper Accomodation Class
        /// </summary>
        public AccomodationClass SleeperClass { get; set; }
        /// <summary>
        ///  Reservation indicator
        /// </summary>
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
            Service = service;
        }

        public IReadOnlyList<ScheduleLocation> Locations => _locations;
        
        private List<ScheduleLocation> _locations = new List<ScheduleLocation>(8);

        internal void AddLocation(ScheduleLocation location) => _locations.Add(location);
        
        public bool RunsOn(DateTime date)
        {
            return Calendar.IsActiveOn(date);
        }

        public bool HasRetailServiceId(string retailServiceId)
        {
            return !string.IsNullOrEmpty(RetailServiceId) && 
                   retailServiceId != null && 
                   retailServiceId.StartsWith(ShortRetailServiceId);
        }

        public bool OperatedBy(string toc)
        {
            return Operator.Equals(toc);
        }
        
        public bool TryFindStop(StopSpecification find, out ScheduleLocation stop)
        {
            stop = Locations.FirstOrDefault(l => l.IsStopAt(find));
            return stop != default;
        }
        
        public override string ToString()
        {
            return $"{TimetableUid} -{StpIndicator} {Calendar} ({Id})";
        }
    }
}