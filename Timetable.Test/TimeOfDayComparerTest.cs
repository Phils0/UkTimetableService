using System;
using Xunit;

namespace Timetable.Test
{
    public class TimeOfDayComparerTest
    {
        private static readonly TimeSpan PastMidnight = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan TenThirty = new TimeSpan(10, 30, 0);
        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan OneDay = new TimeSpan(24, 0, 0);

        public static TheoryData<Time, int> TimeComparison =>
            new TheoryData<Time, int>()
            {
                {new Time(TenThirty), 0},
                {new Time(TenThirty.Add(OneMinute)), -1},
                {new Time(TenThirty.Subtract(OneMinute)), 1},
                {new Time(TenThirty.Add(OneDay)), 0},
                {new Time(PastMidnight), 1},
                {new Time(PastMidnight.Add(OneDay)), 1},
            };
        
        [Theory]
        [MemberData(nameof(TimeComparison))]
        public void Compare(Time y, int expected)
        {
            var x = new Time(TenThirty);
            var comparer = Time.TimeOfDayComparer;
            
            Assert.Equal(expected, comparer.Compare(x, y));
            Assert.Equal(expected * -1, comparer.Compare(y, x));
        }
    }
}