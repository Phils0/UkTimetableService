using System;
using System.Collections;
using System.Collections.Generic;
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
        
        [Theory]
        [ClassData(typeof(SameTimeTestData))]
        public void IsSameTime(Time y, bool expected)
        {
            var x = new Time(TenThirty);
            var comparer = Time.IsSameTimeComparer;
            
            Assert.Equal(expected, comparer.Equals(x, y));
            Assert.Equal(expected, comparer.Equals(y, x));
        }
        
        [Theory]
        [ClassData(typeof(SameTimeTestData))]
        public void IsSameTimeGetHashCodeIsSameWhenEqual(Time y, bool expected)
        {
            var x = new Time(SameTimeTestData.TenThirty);
            var comparer = Time.IsSameTimeComparer;
            var xHash = comparer.GetHashCode(x);
            
            Assert.Equal(expected, comparer.GetHashCode(y) == xHash);
        }
    }
    
    public class SameTimeTestData : IEnumerable<object[]>
    {
        public static readonly TimeSpan TenThirty = new TimeSpan(10, 30, 0);
        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan OneDay = new TimeSpan(24, 0, 0);
        
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] {new Time(TenThirty), true};
            yield return new object[] {new Time(TenThirty.Add(OneMinute)), false};
            yield return new object[] {new Time(TenThirty.Subtract(OneMinute)), false};
            yield return new object[] {new Time(TenThirty.Add(OneDay)), true};
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}