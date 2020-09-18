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
        /// National Location Code
        /// </summary>
        public string Nlc { get; set; }
        
        /// <summary>
        /// Tiploc locations included
        /// </summary>
        public ISet<Location> Locations { get; set; }
        
        /// <summary>
        /// The tocs that have services that stop here.
        /// </summary>
        public ISet<string> TocServices { get; set; }
        
        public override string ToString()
        {
            return  String.IsNullOrEmpty(ThreeLetterCode) ? "Not Set" : $"{ThreeLetterCode}";
        }
    }
}
