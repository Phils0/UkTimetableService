using System;

namespace Timetable.Web.Model
{       
    /// <summary>
    /// A Service schedule
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Timetable Id
        /// </summary>
        public string TimetableUid { get; set; }
        /// <summary>
        /// Running date
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Service is cancelled on date
        /// </summary>
        public bool IsCancelled { get; set; }
        /// <summary>
        /// Retail Service ID - used by NRS
        /// </summary>
        public string RetailServiceId { get; set; }
        /// <summary>
        /// Train Identity - sometimes called HeadCode
        /// </summary>
        public string TrainIdentity { get; set; }
        /// <summary>
        /// Toc
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// Seat Accomodation class
        /// </summary>
        public string SeatClass { get; set; }
        /// <summary>
        /// Sleeper Accomodation Class
        /// </summary>
        public string SleeperClass { get; set; }
        /// <summary>
        ///  Reservation indicator
        /// </summary>
        public string ReservationIndicator { get; set; }

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
        /// <summary>
        /// Service stops
        /// </summary>
        public ScheduledStop[] Stops { get; set; }
        
        public override string ToString()
        {
            return $"{TimetableUid} {Date:d}";
        }
    }
}