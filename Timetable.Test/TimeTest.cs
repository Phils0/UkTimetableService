using System;
using Xunit;

namespace Timetable.Test
{
    public class TimeTest
    {
        private static readonly TimeSpan TenThirty = new TimeSpan(10, 30, 0);
        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan ThirtySeconds = new TimeSpan(0, 0, 30);
        private static readonly TimeSpan OneDay = new TimeSpan(24, 0, 0);

        [Fact]
        public void ToSting()
        {
            var t = new Time(TenThirty);
            
            Assert.Equal("10:30", t.ToString());
        }
        
        [Fact]
        public void ToStingWithSeconds()
        {
            var t = new Time(new TimeSpan(10, 30,30));
            
            Assert.Equal("10:30:30", t.ToString());
        }
        
        [Fact]
        public void ToStingNextDay()
        {
            var t = new Time(new TimeSpan(25, 10 ,0));
            
            Assert.Equal("01:10 (+1)", t.ToString());
        }

        // Add and Subtract helper methods, not fully tested e.g. when goes negative
        [Fact]
        public void AddTimespan()
        {
            var t = new Time(TenThirty);
            var tAdd = t.Add(OneMinute);
            
            Assert.Equal(new Time(new TimeSpan(10, 31,0)), tAdd);
            Assert.Equal(new Time(TenThirty), t);
        }
        
        [Fact]
        public void SubtractTimespan()
        {
            var t = new Time(TenThirty);
            var tSubtract = t.Subtract(OneMinute);
            
            Assert.Equal(new Time(new TimeSpan(10, 29,0)), tSubtract);
            Assert.Equal(new Time(TenThirty), t);
        }
        
        public static TheoryData<Time, Time> TimeAddDay =>
            new TheoryData<Time, Time>()
            {
                {new Time(TenThirty), new Time(TenThirty) },
                {new Time(TenThirty.Add(OneMinute)), new Time(TenThirty.Add(OneMinute)) },
                {new Time(TenThirty.Subtract(OneMinute)), new Time(TenThirty.Subtract(OneMinute).Add(OneDay)) },
                {new Time(TenThirty.Add(ThirtySeconds)), new Time(TenThirty.Add(ThirtySeconds)) },
                {new Time(TenThirty.Subtract(ThirtySeconds)), new Time(TenThirty.Subtract(ThirtySeconds).Add(OneDay)) },
            };
        
        private static readonly Time Start =  new Time(TenThirty);
        
        [Theory]
        [MemberData(nameof(TimeAddDay))]
        public void AddDay(Time time, Time expected)
        {
            var tAdd = time.MakeAfterByAddingADay(Start);
           
            Assert.Equal(expected, tAdd);
        }
        
        public static TheoryData<Time, bool> TimeComparisons =>
            new TheoryData<Time, bool>()
            {
                {new Time(TenThirty), false },
                {new Time(TenThirty.Add(OneMinute)), false },
                {new Time(TenThirty.Subtract(OneMinute)), true },
                {new Time(TenThirty.Add(ThirtySeconds)), false },
                {new Time(TenThirty.Subtract(ThirtySeconds)), true },
            };

        [Theory]
        [MemberData(nameof(TimeComparisons))]
        public void IsBefore(Time time, bool expected)
        {
            Assert.Equal(expected, time.IsBefore(Start));
        }
    }
}