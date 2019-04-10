using System;
using Xunit;

namespace Timetable.Test
{
    public class TimeTest
    {
        [Fact]
        public void ToSting()
        {
            var t = new Time(new TimeSpan(10, 30 ,0));
            
            Assert.Equal("10:30", t.ToString());
        }
        
        [Fact]
        public void ToStingNextDay()
        {
            var t = new Time(new TimeSpan(10, 30 ,0), 1);
            
            Assert.Equal("10:30 (+1)", t.ToString());
        }
    }
}