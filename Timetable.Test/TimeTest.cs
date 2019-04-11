using System;
using Xunit;

namespace Timetable.Test
{
    public class TimeTest
    {
        private static readonly TimeSpan TenThirty = new TimeSpan(10, 30, 0);
        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);

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
            var t = new Time(TenThirty, 1);
            
            Assert.Equal("10:30 (+1)", t.ToString());
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
        
    }
}