using System;

namespace Timetable.Web.Model
{
    public class ServiceNotFound
    {
        /// <summary>
        /// Timetable Id
        /// </summary>
        public string TimetableUid { get; set; }
        /// <summary>
        /// Timetable Id
        /// </summary>
        public string RetailServiceid { get; set; }
        /// <summary>
        /// Running date
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Reason not found
        /// </summary>
        public string Reason { get; set; }
    }
}