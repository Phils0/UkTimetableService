using System;

namespace Timetable.Web.Model
{
    /// <summary>
    /// Location in a schedule
    /// </summary>
    public class ScheduleLocation
    {
        /// <summary>
        /// Tiploc code
        /// </summary>
        public string Tiploc { get; set; }
        
        /// <summary>
        /// CRS code
        /// </summary>
        public string ThreeLetterCode { get; set; }
        
        public override string ToString()
        {
            return $"{ThreeLetterCode}-{Tiploc}";
        }
    }
}