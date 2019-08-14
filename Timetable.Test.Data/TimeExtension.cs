using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using ReflectionMagic;

namespace Timetable
{
    public static class TimeExtension
    {
        [Pure]
        public static Time Add(this Time time, TimeSpan val) => time.AsDynamic().Add(val);
        [Pure]
        public static Time Subtract(this Time time, TimeSpan val) => new Time(time.Value.Subtract(val));
        [Pure]
        public static Time AddMinutes(this Time time, int minutes) => Add(time, new TimeSpan(0, minutes, 0));
        [Pure]
        public static Time AddDay(this Time time) => Add(time, Time.OneDay);
    }
}