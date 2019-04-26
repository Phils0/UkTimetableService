using System;

namespace Timetable.Web.Model
{
    public class LocationTimetable
    {
        /// <summary>
        /// Request
        /// </summary>
        public LocationTimetableRequest Request { get; set; }

        /// <summary>
        /// Time Board was created
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Services arriving/departing
        /// </summary>
        public TimetableItem[] Services { get; set; } = new TimetableItem[0];
    }
}