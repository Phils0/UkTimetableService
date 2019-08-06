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
    public class FoundResponse : SearchResponse
    {
        /// <summary>
        /// Services arriving/departing
        /// </summary>
        public FoundItem[] Services { get; set; } = new FoundItem[0];
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