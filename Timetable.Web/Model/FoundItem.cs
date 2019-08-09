namespace Timetable.Web.Model
{
    /// <summary>
    /// Departure/Arrival
    /// </summary>
    public class FoundItem
    {
        /// <summary>
        /// Service Details
        /// </summary>
        public ServiceSummary Service { get; set; }

        /// <summary>
        /// Service times at
        /// </summary>
        public ScheduledStop At { get; set; }

        /// <summary>
        /// Details for Coming From location - Arrivals only
        /// </summary>
        public ScheduledStop ComingFrom { get; set; }

        /// <summary>
        /// Details for Going To location - Departures only
        /// </summary>
        public ScheduledStop GoingTo { get; set; }
    }
}