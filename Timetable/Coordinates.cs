using System;

namespace Timetable
{
    /// <summary>
    /// Location coordinates
    /// </summary>
    public class Coordinates : IEquatable<Coordinates>
    {
        /// <summary>
        /// OS Easting value
        /// </summary>
        public int Eastings { get; set; }
        /// <summary>
        /// ES Nothings value
        /// </summary>
        public int Northings { get; set; }
        /// <summary>
        /// Estimated location
        /// </summary>
        public bool IsEstimate { get; set; }
        
        public bool Equals(Coordinates other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Eastings == other.Eastings && Northings == other.Northings;
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
                return (Eastings * 397) ^ Northings;
            }
        }
        
        public override string ToString()
        {
            return IsEstimate ? $"{Eastings},{Northings} (E)" :  $"{Eastings},{Northings}";
        }
    }
}