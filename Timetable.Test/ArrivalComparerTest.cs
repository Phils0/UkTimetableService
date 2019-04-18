using System;
using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ArrivalComparerTest
    {
        private static readonly Time PastMidnight = new Time( new TimeSpan(0, 1, 0));
        private static readonly Time ElevenThirty = new Time(new TimeSpan(23, 30, 0));
        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan OneDay = new TimeSpan(24, 0, 0);

        public static TheoryData<IArrival, int> ArrivalComparison =>
            new TheoryData<IArrival, int>()
            {
                {CreateStop(ElevenThirty), 0},
                {CreateStop(ElevenThirty, id: 2), -1},
                {CreateStop(Time.NotValid, ElevenThirty), 0},
                {CreateStop(ElevenThirty.Add(OneMinute)), 1},
                {CreateStop(ElevenThirty.Subtract(OneMinute)), -1},
                {CreateStop(PastMidnight), -1},
                {CreateStop(PastMidnight.Add(OneDay)), -1},
                {CreateDestination(ElevenThirty), 0},
                {CreateDestination(ElevenThirty, id: 2), -1},
                {CreateDestination(Time.NotValid, ElevenThirty), 0},
                {CreateDestination(ElevenThirty.Add(OneMinute)), 1},
                {CreateDestination(ElevenThirty.Subtract(OneMinute)), -1},
                {CreateDestination(PastMidnight), -1},
                {CreateDestination(PastMidnight.Add(OneDay)), -1},
            };
    
        private static IArrival CreateStop(Time time, Time workingTime = default(Time), int id = 1 )
        {
            workingTime = workingTime.Equals(default(Time)) ? time : workingTime;
            
            return new ScheduleStop()
            {
                Arrival = time,
                WorkingArrival = workingTime,
                Id = id,
            };
        }
        
        private static IArrival CreateDestination(Time time, Time workingTime = default(Time), int id = 1 )
        {
            workingTime = workingTime.Equals(default(Time)) ? time : workingTime;
            
            return new ScheduleDestination()
            {
                Arrival = time,
                WorkingArrival = workingTime,
                Id = id,
            };
        }
        
        [Theory]
        [MemberData(nameof(ArrivalComparison))]
        public void Compare(IArrival y, int expected)
        {
            var x = CreateStop(ElevenThirty);
            var comparer = new ArrivalComparer();
            
            Assert.Equal(expected, comparer.Compare(x, y));
            Assert.Equal(expected * -1, comparer.Compare(y, x));
        }
    }
}