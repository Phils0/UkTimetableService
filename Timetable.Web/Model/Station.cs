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
        /// <remarks>This is set from Stations knowledgebase not the location data. 
        /// We could use the Main location NLC from the CIF however it is potentially misleading
        /// for some stations e.g. Bristol Airport where the fare NLC associated with the location is different to the timetable
        /// NLC which implies it is part of Bristol Temple Meads (probably because its the bus stop) so for now will leave it as null
        /// unless there is a value in the Knowledgebase</remarks>
        public string Nlc { get; set; }
        
        /// <summary>
        /// Station Name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Coordinates
        /// </summary>
        public Coordinates Coordinates { get; set; }
        
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
