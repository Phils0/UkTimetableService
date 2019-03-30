using System;

namespace Timetable
{
    /// <summary>
    /// Interchange status
    /// </summary>
    public enum InterchangeStatus
    {
        NotAnInterchange = 0,
        Minor = 1,
        Normal = 2,
        Main = 3,
        SubsidiaryLocation = 9      
    }
    
    /// <summary>
    /// Location from the Master Station List
    /// </summary>
    public class Location
    {
        public static Location NotSet = new Location()
        {
            Tiploc = "",
            ThreeLetterCode = "",
        };
        
        /// <summary>
        /// Tiploc code
        /// </summary>
        public string Tiploc { get; set; }
        
        /// <summary>
        /// CRS code
        /// </summary>
        public string ThreeLetterCode { get; set; }
        
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Coordinates
        /// </summary>
        public Coordinates Coordinates { get; set; }

        /// <summary>
        /// Interchange Status
        /// </summary>
        /// <remarks>
        /// 0 - not an interchange
        /// 1, 2, 3 Higher number more important interchange
        /// 9 a subsidiary TIPLOC at a station which has more than one TIPLOC.
        /// Stations which have more than one TIPLOC always have the same principal 3-Letter Code.</remarks>
        public InterchangeStatus InterchangeStatus { get; set; }

        public bool IsSubsidiary => InterchangeStatus.SubsidiaryLocation.Equals(InterchangeStatus);
                
        /// <summary>
        /// Containing station
        /// </summary>
        public Station Station { get; set; }
        
        public override string ToString()
        {
            return String.IsNullOrEmpty(Tiploc) ? "Not Set" : $"{ThreeLetterCode}-{Tiploc}";
        }
    }
}