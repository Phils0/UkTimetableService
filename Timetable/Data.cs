using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// Data container to hold loaded timetable
    /// </summary>
    public class Data
    {
        /// <summary>
        /// Stations
        /// </summary>
        public IDictionary<string, Station> Locations { get; set; }
    }
}