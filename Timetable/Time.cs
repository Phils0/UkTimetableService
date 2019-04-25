using System;
using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// Represents a time in the timetable
    /// </summary>
    public struct Time : IEquatable<Time>
    {
        private sealed class EarlierToLaterComparer : IComparer<Time>
        {
            public int Compare(Time x, Time y)
            {
                var compare = x.Value.Hours.CompareTo(y.Value.Hours);
                if (compare != 0)
                    return compare;
                
                compare = x.Value.Minutes.CompareTo(y.Value.Minutes);
                if (compare != 0)
                    return compare;

                return x.Value.Seconds.CompareTo(y.Value.Seconds);
            }
        }
        
        private sealed class LaterToEarlierComparer : IComparer<Time>
        {
            private readonly IComparer<Time> _comparer = new EarlierToLaterComparer();
            
            public int Compare(Time x, Time y)
            {
                return _comparer.Compare(y, x);
            }
        }
        
        /// <summary>
        /// Earlier to Later comparer
        /// </summary>
        /// <remarks>Ignores going over into the next day, therefore 00:10+1day is less than 23:50 </remarks>
        public static IComparer<Time> EarlierLaterComparer => new EarlierToLaterComparer();

        /// <summary>
        /// Later to Earlier comparer
        /// </summary>
        /// <remarks>Ignores going over into the next day, therefore 00:10+1day is less than 23:50 </remarks>
        public static IComparer<Time> LaterEarlierComparer => new LaterToEarlierComparer();
        
        
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

        public Time AddMinutes(int minutes) => Add(new TimeSpan(0, minutes, 0));

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