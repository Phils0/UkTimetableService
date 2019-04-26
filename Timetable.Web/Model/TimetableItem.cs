namespace Timetable.Web.Model
{
    public class TimetableItem
    {
        /// <summary>
        /// Service Details
        /// </summary>
        public Service Service { get; set; }

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