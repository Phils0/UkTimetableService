namespace Timetable.Web.Model
{
    public class LocationTimetableItem
    {
        /// <summary>
        /// Service Details
        /// </summary>
        public ServiceSummary Service { get; set; }

        /// <summary>
        /// Service times at
        /// </summary>
        public ScheduleStop At { get; set; }

        /// <summary>
        /// Details for Coming From location - Arrivals only
        /// </summary>
        public ScheduleStop ComingFrom { get; set; }

        /// <summary>
        /// Details for Going To location - Departures only
        /// </summary>
        public ScheduleStop GoingTo { get; set; }
    }
}