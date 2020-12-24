using System.Collections.Generic;

namespace Timetable
{
    public class RealtimeData
    {
        public string Timetable { get; set; }
        
        public string Reference { get; set; }
        
        public IReadOnlyDictionary<int, string> CancelReasons { get; set; }
        
        public IReadOnlyDictionary<int, string> DelayReasons { get; set; }
    }
}