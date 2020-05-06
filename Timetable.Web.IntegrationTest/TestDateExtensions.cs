using System;

namespace Timetable.Web.IntegrationTest
{
    public static class TestDateExtensions
    {
        public static DateTime NextWednesday(this DateTime dt)
        {
            if(dt.DayOfWeek == DayOfWeek.Wednesday)
                return dt.AddDays(7);
            if (dt.DayOfWeek < DayOfWeek.Wednesday)
                return dt.AddDays(7 + (DayOfWeek.Wednesday - dt.DayOfWeek));

            return dt.AddDays(7 - (dt.DayOfWeek - DayOfWeek.Wednesday));
        }
    }
}