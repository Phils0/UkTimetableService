using System;
using Xunit;

namespace Timetable.Test
{
    public class TimeComparerTest
    {
        private static readonly TimeSpan PastMidnight = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan TenThirty = new TimeSpan(10, 30, 0);
        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan OneDay = new TimeSpan(24, 0, 0);

        public static TheoryData<Time, int> TimeComparisonTests =>
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
        [MemberData(nameof(TimeComparisonTests))]
        public void EarlierLaterCompare(Time y, int expected)
        {
            var x = new Time(TenThirty);
            var comparer = Time.EarlierLaterComparer;
            
            Assert.Equal(expected, comparer.Compare(x, y));
            Assert.Equal(expected * -1, comparer.Compare(y, x));
        }
        
        [Theory]
        [MemberData(nameof(TimeComparisonTests))]
        public void LaterEarlierCompare(Time y, int inverted)
        {
            var expected = inverted * -1;
            var x = new Time(TenThirty);
            var comparer = Time.LaterEarlierComparer;
            
            Assert.Equal(expected, comparer.Compare(x, y));
            Assert.Equal(expected * -1, comparer.Compare(y, x));
        }
        
        public static TheoryData<Time, bool> SameTimeTests =>
            new TheoryData<Time, bool>()
            {
                {new Time(TenThirty), true},
                {new Time(TenThirty.Add(OneMinute)), false},
                {new Time(TenThirty.Subtract(OneMinute)), false},
                {new Time(TenThirty.Add(OneDay)), true}
            };
        
        [Theory]
        [MemberData(nameof(SameTimeTests))]
        public void IsSameTime(Time y, bool expected)
        {
            var x = new Time(TenThirty);
            var comparer = Time.IsSameTimeComparer;
            
            Assert.Equal(expected, comparer.Equals(x, y));
            Assert.Equal(expected, comparer.Equals(y, x));
        }
        
        [Theory]
        [MemberData(nameof(SameTimeTests))]
        public void IsSameTimeGetHashCodeIsSameWhenEqual(Time y, bool expected)
        {
            var x = new Time(TenThirty);
            var comparer = Time.IsSameTimeComparer;
            var xHash = comparer.GetHashCode(x);
            
            Assert.Equal(expected, comparer.GetHashCode(y) == xHash);
        }
    }
}