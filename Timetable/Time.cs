using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Timetable
{
    /// <summary>
    /// Represents a time in the timetable
    /// </summary>
    public struct Time : IEquatable<Time>
    {
        // Rail day assumed to start at 02:30 
        public static Time StartRailDay = new Time(new TimeSpan(2, 30,0));
        public static Time Midnight = new Time(new TimeSpan(0, 0,0));
        
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

        private sealed class SameTimeEqualityComparer : IEqualityComparer<Time>
        {
            public bool Equals(Time x, Time y)
            {
                var xValue = x.Value;
                var yValue = y.Value;

                return xValue.Hours == yValue.Hours &&
                       x.Value.Minutes == yValue.Minutes &&
                       xValue.Seconds == yValue.Seconds;
            }
            public int GetHashCode(Time obj)
            {
                unchecked
                {
                    var v = obj.Value;
                    var hashCode = v.Hours;
                    hashCode = (hashCode * 397) ^ v.Minutes;
                    hashCode = (hashCode * 397) ^ v.Seconds;
                    return hashCode;
                }
            }
        }
        
        /// <summary>
        /// Earlier to Later comparer
        /// </summary>
        /// <remarks>Ignores going over into the next day, therefore 00:10+1day is less than 23:50 </remarks>
        public static readonly IComparer<Time> EarlierLaterComparer = new EarlierToLaterComparer();

        /// <summary>
        /// Later to Earlier comparer
        /// </summary>
        /// <remarks>Ignores going over into the next day, therefore 00:10+1day is less than 23:50 </remarks>
        public static readonly IComparer<Time> LaterEarlierComparer = new LaterToEarlierComparer();

        /// <summary>
        /// Is same time equality
        /// </summary>
        /// <remarks>Ignores going over into the next day, therefore 00:10+1day equals 00:10 </remarks>
        public static readonly IEqualityComparer<Time> IsSameTimeComparer = new SameTimeEqualityComparer();
        
        public static readonly TimeSpan OneDay = new TimeSpan(24, 0, 0);
       
        public static readonly Time NotValid = new Time(TimeSpan.Zero); 

        public TimeSpan Value { get; }
        public bool IsNextDay => Value > OneDay;

        public bool IsValid => !(Equals(NotValid)  || IsBefore(NotValid));
        
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
        
        public bool IsBeforeIgnoringDay(Time other)
        {
            return EarlierLaterComparer.Compare(this, other) < 0;
        }
             
        public Time MakeAfterByAddingADay(Time start) => IsBefore(start) && IsValid ? Add(OneDay) : this;

        private Time Add(TimeSpan ts) => new Time(this.Value.Add(ts));
        
        public bool Equals(Time other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Time other && Equals(other);
        }

        public bool IsSameTime(Time other)
        {
            return  IsSameTimeComparer.Equals(this, other);
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        
        public override string ToString()
        {
            var negative = (Value < TimeSpan.Zero ?  "-" : "");
            var timeString = Value.Seconds == 0 ? $"{negative}{Value:hh\\:mm}" : $"{negative}{Value:hh\\:mm\\:ss}";

            return IsNextDay ? $"{timeString} (+1)" : timeString;
        }
        
        public static Time Parse(string time) => new Time(TimeSpan.Parse(time));
    }
}