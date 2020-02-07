using System;

namespace Timetable.Web.Test
{
    internal static class TestTime
    {
        internal static readonly TimeSpan Ten = new TimeSpan(10, 0, 0);
        internal static readonly TimeSpan TenFifteen = new TimeSpan(10, 15, 0);
        internal static readonly TimeSpan TenTwenty = new TimeSpan(10, 20, 0);
        internal static readonly TimeSpan TenTwentyFive = new TimeSpan(10, 25, 0);
        internal static readonly TimeSpan TenThirty = new TimeSpan(10, 30, 0);
        
        internal static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        internal static readonly TimeSpan ThirtySeconds = new TimeSpan(0, 0, 30); 
        
        internal static readonly DateTime August1 = new DateTime(2019, 8, 1);
    }
}