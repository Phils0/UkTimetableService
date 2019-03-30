using System;
using System.Collections.Generic;

namespace Timetable.Web.Model
{
    /// <summary>
    /// Station aggregate
    /// </summary>
    public class Station
    {
        /// <summary>
        /// Identifying Three letter code (CRS)
        /// </summary>
        public string ThreeLetterCode { get; set; }
        
        /// <summary>
        /// Tiploc locations included
        /// </summary>
        public ISet<Location> Locations { get; set; }
        
        public override string ToString()
        {
            return  String.IsNullOrEmpty(ThreeLetterCode) ? "Not Set" : $"{ThreeLetterCode}";
        }
    }
}
