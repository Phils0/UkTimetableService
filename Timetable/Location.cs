using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    /// <summary>
    /// Interchange status
    /// </summary>
    public enum InterchangeStatus
    {
        NotSet = -1,
        NotAnInterchange = 0,
        Minor = 1,
        Normal = 2,
        Main = 3,
        SubsidiaryLocation = 9
    }

    /// <summary>
    /// Location from the Master Station List
    /// </summary>
    public class Location : IEquatable<Location>, IComparable<Location>
    {
        public int CompareTo(Location other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Tiploc, other.Tiploc, StringComparison.Ordinal);
        }

        public bool Equals(Location other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Tiploc, other.Tiploc);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            return Tiploc.GetHashCode();
        }

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
        /// National Location Code - full 6 character code
        /// </summary>
        public string Nlc { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Coordinates
        /// </summary>
        public OrdnanceSurveyCoordinates Coordinates { get; set; }

        /// <summary>
        /// Interchange Status
        /// </summary>
        /// <remarks>
        /// 0 - not an interchange
        /// 1, 2, 3 Higher number more important interchange
        /// 9 a subsidiary TIPLOC at a station which has more than one TIPLOC.
        /// Stations which have more than one TIPLOC always have the same principal 3-Letter Code.</remarks>
        public InterchangeStatus InterchangeStatus { get; set; } = InterchangeStatus.NotSet;

        //HACK This is used to indicate whether we return the location or not
        //TODO This property probably needs removing but needs some thought
        /// <summary>
        /// Whether known (in the Station Master list)
        /// </summary>
        /// <returns></returns>
        public bool IsActive { get; set; } = true;

        public bool IsSubsidiary => InterchangeStatus.SubsidiaryLocation.Equals(InterchangeStatus);
        
        public bool HasThreeLetterCode => !string.IsNullOrEmpty(ThreeLetterCode);

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