using System;
using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class DepartureComparerTest
    {
        private static readonly Time PastMidnight = new Time( new TimeSpan(0, 1, 0));
        private static readonly Time ElevenThirty = new Time(new TimeSpan(23, 30, 0));
        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan OneDay = new TimeSpan(24, 0, 0);

        public static TheoryData<IDeparture, int> DepartureComparison =>
            new TheoryData<IDeparture, int>()
            {
                {CreateStop(ElevenThirty), 0},
                {CreateStop(ElevenThirty, id: 2), -1},
                {CreateStop(Time.NotValid, ElevenThirty), 0},
                {CreateStop(ElevenThirty.Add(OneMinute)), -1},
                {CreateStop(ElevenThirty.Subtract(OneMinute)), 1},
                {CreateStop(PastMidnight), 1},
                {CreateStop(PastMidnight.Add(OneDay)), 1},
                {CreateOrigin(ElevenThirty), 0},
                {CreateOrigin(ElevenThirty, id: 2), -1},
                {CreateOrigin(Time.NotValid, ElevenThirty), 0},
                {CreateOrigin(ElevenThirty.Add(OneMinute)), -1},
                {CreateOrigin(ElevenThirty.Subtract(OneMinute)), 1},
                {CreateOrigin(PastMidnight), 1},
                {CreateOrigin(PastMidnight.Add(OneDay)), 1},
            };
    
        private static IDeparture CreateStop(Time time, Time workingTime = default(Time), int id = 1 )
        {
            workingTime = workingTime.Equals(default(Time)) ? time : workingTime;
            
            return new ScheduleStop()
            {
                Departure = time,
                WorkingDeparture = workingTime,
                Id = id,
            };
        }
        
        private static IDeparture CreateOrigin(Time time, Time workingTime = default(Time), int id = 1 )
        {
            workingTime = workingTime.Equals(default(Time)) ? time : workingTime;
            
            return new ScheduleOrigin()
            {
                Departure = time,
                WorkingDeparture = workingTime,
                Id = id,
            };
        }
        
        [Theory]
        [MemberData(nameof(DepartureComparison))]
        public void Compare(IDeparture y, int expected)
        {
            var x = CreateStop(ElevenThirty);
            var comparer = new DepartureComparer();
            
            Assert.Equal(expected, comparer.Compare(x, y));
            Assert.Equal(expected * -1, comparer.Compare(y, x));
        }
    }
}