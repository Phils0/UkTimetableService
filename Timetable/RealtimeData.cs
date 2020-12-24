using System.Collections.Generic;

namespace Timetable
{
    public class RealtimeData
    {
        /// <summary>
        /// Darwin Timetable file
        /// </summary>
        public string Timetable { get; set; }
        /// <summary>
        /// Darwin Reference file
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// Darwin Cancellation Reasons
        /// </summary>
        public IReadOnlyDictionary<int, string> CancelReasons { get; set; }
        /// <summary>
        /// Darwin Late Running Reasons
        /// </summary>
        public IReadOnlyDictionary<int, string> LateRunningReasons { get; set; }
    }
}