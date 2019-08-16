using System;

namespace Timetable.Web.Model
{
    /// <summary>
    /// Service not found response
    /// </summary>
    public class ServiceNotFound
    {
        /// <summary>
        /// Timetable Id or Retail ServiceId
        /// </summary>
        public string Id { get; set; }
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