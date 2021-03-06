using System;

namespace Timetable
{
    /// <summary>
    /// Location coordinates (loingitude, latitude
    /// </summary>
    public class Coordinates : IEquatable<Coordinates>
    {
        /// <summary>
        /// Longitude
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        public decimal Latitude { get; set; }
        /// <summary>
        /// Estimated location
        /// </summary>
        public bool IsEstimate { get; set; }
        
        public bool Equals(Coordinates other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Longitude == other.Longitude && Latitude == other.Latitude;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coordinates) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Longitude.GetHashCode() * 397) ^ Latitude.GetHashCode();
            }
        }
        
        public override string ToString()
        {
            return IsEstimate ? $"{Longitude},{Latitude} (E)" :  $"{Longitude},{Latitude}";
        }
    }
}