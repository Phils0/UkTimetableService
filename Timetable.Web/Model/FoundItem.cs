namespace Timetable.Web.Model
{
    /// <summary>
    /// Departure/Arrival
    /// </summary>
    public abstract class FoundItem
    {
        /// <summary>
        /// Service times at
        /// </summary>
        public ScheduledStop At { get; set; }

        /// <summary>
        /// Details for Coming From location - Arrivals only
        /// </summary>
        public ScheduledStop From { get; set; }

        /// <summary>
        /// Details for Going To location - Departures only
        /// </summary>
        public ScheduledStop To { get; set; }
        
        /// <summary>
        /// Whether found journey includes splitting\joining 
        /// </summary>
        public IncludedAssociation Association { get; set; }
        
        /// <summary>
        /// Via information for service at location
        /// </summary>
        public string ViaText { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Departure/Arrival
    /// </summary>
    public class FoundSummaryItem : FoundItem
    {
        /// <summary>
        /// Service Details
        /// </summary>
        public ServiceSummary Service { get; set; }
    }
    
    /// <summary>
    /// Departure/Arrival
    /// </summary>
    public class FoundServiceItem : FoundItem
    {
        /// <summary>
        /// Service Details
        /// </summary>
        public Service Service { get; set; }
    }
}