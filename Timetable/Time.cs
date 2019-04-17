using System;

namespace Timetable
{
    /// <summary>
    /// Represents a time in the timetable
    /// </summary>
    public struct Time : IEquatable<Time>
    {
        private static readonly TimeSpan OneDay = new TimeSpan(24, 0, 0);
       
        public static readonly Time NotValid = new Time(TimeSpan.Zero); 

        public TimeSpan Value { get; }
        public bool IsNextDay => Value > OneDay;

        public bool IsValid => !Equals(NotValid);
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time">Time</param>
        public Time(TimeSpan time)
        {
            Value = time;
        }

        public bool IsBefore(Time other)
        {
            return Value < other.Value;
        }
             
        public Time MakeAfterByAddingADay(Time start) => IsBefore(start) && IsValid ? Add(OneDay) : this;

        public Time Add(TimeSpan ts) => new Time(this.Value.Add(ts));
        public Time Subtract(TimeSpan ts) => new Time(this.Value.Subtract(ts));

        public bool Equals(Time other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Time other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            var timeString = Value.Seconds == 0 ? $"{Value:hh\\:mm}" : $"{Value:hh\\:mm\\:ss}";

            return IsNextDay ? $"{timeString} (+1)" : timeString;
        }
    }
}