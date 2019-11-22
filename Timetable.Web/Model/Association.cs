using System;

namespace Timetable.Web.Model
{
    
    public class Association
    {
        /// <summary>
        /// Other Timetable Id
        /// </summary>
        public string TimetableUid { get; set; }
        
        /// <summary>
        /// Running date
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Split or Join
        /// </summary>
        public string AssociationCategory { get; set; }
        
        /// <summary>
        /// Split/Join location
        /// </summary>
        public ScheduledStop Stop { get; set; }
    }
}