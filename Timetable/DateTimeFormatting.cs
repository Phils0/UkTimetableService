using System;

namespace Timetable
{
    public static class DateTimeFormatting
    {
        public static string ToYMD(this DateTime date) => date.ToString("yyyy-MM-dd");
        public static string ToYMDHms(this DateTime date) => date.ToString("yyyy-MM-dd HH:mm:ss");
        public static string ToHm(this DateTime date) => date.ToString("HH:mm");
    }
}
