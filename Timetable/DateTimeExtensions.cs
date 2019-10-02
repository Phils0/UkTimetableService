using System;

namespace Timetable
{
    internal static class DateTimeExtensions
    {
        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);

        internal static DateTime RoundToMinute(this DateTime dateTime)
        {
            return Round(dateTime, OneMinute);
        }

        internal static DateTime Round(this DateTime dateTime, TimeSpan interval)
        {
            var halfIntervelTicks = ((interval.Ticks + 1) >> 1);

            return dateTime.AddTicks(halfIntervelTicks - ((dateTime.Ticks + halfIntervelTicks) % interval.Ticks));
        }
    }
}
