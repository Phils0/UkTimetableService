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
        
        [Fact]
        public void ToStingNegative()
        {
            var t = Time.Midnight.AddMinutes(-1);
            
            Assert.Equal("-00:01", t.ToString());
        }
        
        public static TheoryData<Time, Time> TimeAddDay =>
            new TheoryData<Time, Time>()
            {
                {Time.NotValid, Time.NotValid },
                {new Time(TenThirty), new Time(TenThirty) },
                {new Time(TenThirty.Add(OneMinute)), new Time(TenThirty.Add(OneMinute)) },
                {new Time(TenThirty.Subtract(OneMinute)), new Time(TenThirty.Subtract(OneMinute).Add(OneDay)) },
                {new Time(TenThirty.Add(ThirtySeconds)), new Time(TenThirty.Add(ThirtySeconds)) },
                {new Time(TenThirty.Subtract(ThirtySeconds)), new Time(TenThirty.Subtract(ThirtySeconds).Add(OneDay)) },
            };
        
        private static readonly Time Start =  new Time(TenThirty);
        
        [Theory]
        [MemberData(nameof(TimeAddDay))]
        public void AddDayIfStopNextDay(Time time, Time expected)
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

        public static TheoryData<Time, bool> IsValidTests =>
            new TheoryData<Time, bool>()
            {
                {Time.NotValid, false },
                {new Time(TimeSpan.Zero), false },
                {new Time(TenThirty), true },
                {Time.Midnight, false},
                {Time.Midnight.AddMinutes(-1), false}
            };
        
        [Theory]
        [MemberData(nameof(IsValidTests))]
        public void IsValid(Time time, bool expected)
        {
            Assert.Equal(expected, time.IsValid);
        }
        
        public static TheoryData<Time, bool> AroundMidnight =>
            new TheoryData<Time, bool>()
            {
                {new Time(new TimeSpan(0, 1, 0)), false },
                {new Time(new TimeSpan(23, 59, 0)), false },
                {new Time(OneDay), false },
                {new Time(OneDay.Add(OneMinute)), true }
            };

        [Theory]
        [MemberData(nameof(AroundMidnight))]
        public void IsNextDay(Time time, bool expected)
        {
            Assert.Equal(expected, time.IsNextDay);
        }
        
        [Theory]
        [ClassData(typeof(SameTimeTestData))]
        public void IsSameTime(Time other, bool expected)
        {
            var time = new Time(TenThirty);
            var comparer = Time.IsSameTimeComparer;
            
            Assert.Equal(expected, time.IsSameTime(other));
        }

        public static TheoryData<string, Time> TimesToParse =>
            new TheoryData<string, Time>()
            {
                {"00:00", new Time(new TimeSpan(0, 0, 0)) },
                {"02:30", new Time(new TimeSpan(2, 30, 0)) },
                {"12:00", new Time(new TimeSpan(12, 0, 0)) },
                {"23:59", new Time(new TimeSpan(23, 59, 0)) },
                {"2:30", new Time(new TimeSpan(2, 30, 0)) },
            };
        
        [Theory]
        [MemberData(nameof(TimesToParse))]
        public void ParseTime(string time, Time expected)
        {
            var parsed = Time.Parse(time);
            Assert.Equal(expected, parsed);
        }
        
    }
}