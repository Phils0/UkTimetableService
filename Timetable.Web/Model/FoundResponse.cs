using System;

namespace Timetable.Web.Model
{
    /// <summary>
    /// Generic search response
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// Request
        /// </summary>
        public SearchRequest Request { get; set; }

        /// <summary>
        /// Time Board was created
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }
    
    /// <summary>
    /// Found Departures/Arrivals
    /// </summary>
    public class FoundServiceResponse : SearchResponse
    {
        /// <summary>
        /// Services arriving/departing
        /// </summary>
        public FoundServiceItem[] Services { get; set; } = new FoundServiceItem[0];
    }
    
    /// <summary>
    /// Found Departures/Arrivals
    /// </summary>
    public class FoundSummaryResponse : SearchResponse
    {
        /// <summary>
        /// Services arriving/departing
        /// </summary>
        public FoundSummaryItem[] Services { get; set; } = new FoundSummaryItem[0];
    }
    
    /// <summary>
    /// Service not returned response
    /// </summary>
    public class NotFoundResponse : SearchResponse
    {
        /// <summary>
        /// Reason not found
        /// </summary>
        public string Reason { get; set; }
    }
}