using System;

namespace Timetable.Web.Model
{
    /// <summary>
    /// Service not returned response
    /// </summary>
    public class ServiceNotReturned
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
    
    /// <summary>
    /// Service not found
    /// </summary>
    public class ServiceNotFound : ServiceNotReturned
    {
    }
    
    /// <summary>
    /// Service cancelled
    /// </summary>
    public class ServiceCancelled : ServiceNotReturned
    {
    }
}