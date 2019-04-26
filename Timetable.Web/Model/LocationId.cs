using System;

namespace Timetable.Web.Model
{
    /// <summary>
    /// Location Id
    /// </summary>
    public class LocationId
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