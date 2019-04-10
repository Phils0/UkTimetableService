using System;

namespace Timetable
{
    /// <summary>
    /// Represents a time in the timetable
    /// </summary>
    public struct Time : IEquatable<Time>
    {
        public TimeSpan Value { get; }
        public int PlusDay { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="plusDay">Add days</param>
        public Time(TimeSpan time, int plusDay = 0)
        {
            Value = time;
            PlusDay = plusDay;
        }

        public bool Equals(Time other)
        {
            return Value.Equals(other.Value) && PlusDay == other.PlusDay;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Time other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode() * 397) ^ PlusDay;
            }
        }

        public override string ToString()
        {
            return PlusDay == 0 ?
                $"{Value:hh\\:mm}" :
                $"{Value:hh\\:mm} (+{PlusDay})";
        }
    }
}