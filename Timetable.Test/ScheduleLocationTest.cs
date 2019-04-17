using System;
using System.Collections.Generic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ScheduleLocationTest
    {
        private static readonly Time Start = new Time(new TimeSpan(23, 45,0 )); 
        private static readonly Time Test = new Time(new TimeSpan(0, 12,0 )); 
        private static readonly Time Expected = new Time(new TimeSpan(24, 12,0 ));

        private static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        private static readonly TimeSpan ThirtySeconds = new TimeSpan(0, 0, 30);     
        
        [Fact]
        public void AddDayToScheduleOrigin()
        {
            var scheduleLocation = TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, Test);
            scheduleLocation.AddDay(Start);
            
            Assert.Equal(Expected, scheduleLocation.Departure);
            Assert.Equal(Expected.Add(ThirtySeconds), scheduleLocation.WorkingDeparture);
        }
        
        [Fact]
        public void DoNotAddDay()
        {
            var scheduleLocation = TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, Start);
            scheduleLocation.AddDay(Start);
            
            Assert.Equal(Start, scheduleLocation.Departure);
            Assert.Equal(Start.Add(ThirtySeconds), scheduleLocation.WorkingDeparture);
        }
        
        [Fact]
        public void AddDayToScheduleStop()
        {
            var scheduleLocation = TestScheduleLocations.CreateStop(TestLocations.Surbiton, Test);
            scheduleLocation.AddDay(Start);
            
            Assert.Equal(Expected, scheduleLocation.Arrival);
            Assert.Equal(Expected.Subtract(ThirtySeconds), scheduleLocation.WorkingArrival);
            Assert.Equal(Expected.Add(OneMinute), scheduleLocation.Departure);
            Assert.Equal(Expected.Add(ThirtySeconds), scheduleLocation.WorkingDeparture);
        }
        
        [Fact]
        public void AddDayOnlyToTimesAfter()
        {
            var arrival = new Time(new TimeSpan(23, 59, 0));
            var departure = new Time(new TimeSpan(0, 1, 0));
            
            var scheduleLocation =  new ScheduleStop()
            {
                Location = TestLocations.Surbiton,
                Sequence = 1,
                Arrival = arrival,
                WorkingArrival = arrival,
                Departure = departure,
                WorkingDeparture = departure,
            };
            
            scheduleLocation.AddDay(Start);
            
            Assert.Equal(arrival, scheduleLocation.Arrival);
            Assert.Equal(arrival, scheduleLocation.WorkingArrival);
 
            var expectedDeparture = new Time(new TimeSpan(24, 1, 0));

            Assert.Equal(expectedDeparture, scheduleLocation.Departure);
            Assert.Equal(expectedDeparture, scheduleLocation.WorkingDeparture);
        }
        
        [Fact]
        public void AddDayToScheduleDestination()
        {
            var scheduleLocation = TestScheduleLocations.CreateDestination(TestLocations.Surbiton, Test);
            scheduleLocation.AddDay(Start);
            
            Assert.Equal(Expected, scheduleLocation.Arrival);
            Assert.Equal(Expected.Subtract(ThirtySeconds), scheduleLocation.WorkingArrival);
        }
        
        [Fact]
        public void AddDayToSchedulePass()
        {
            var scheduleLocation = TestScheduleLocations.CreatePass(TestLocations.Surbiton, Test);
            scheduleLocation.AddDay(Start);
            
            Assert.Equal(Expected, scheduleLocation.PassesAt);
        }

        [Fact]
        public void ScheduleOriginStopTypeIsOrigin()
        {
            var scheduleLocation = TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, Test);
            Assert.Equal(StopType.PickUpOnly, scheduleLocation.AdvertisedStop);
        }
        
        [Fact]
        public void ScheduleDestinationStopTypeIsDestination()
        {
            var scheduleLocation = TestScheduleLocations.CreateDestination(TestLocations.Surbiton, Test);
            Assert.Equal(StopType.SetDownOnly, scheduleLocation.AdvertisedStop);
        }
        
        [Fact]
        public void SchedulePassIsNotAStop()
        {
            var scheduleLocation = TestScheduleLocations.CreatePass(TestLocations.Surbiton, Test);            
            Assert.Equal(StopType.NotAPublicStop, scheduleLocation.AdvertisedStop);
        }
        
        [Theory]
        [InlineData("T", StopType.Normal)]
        [InlineData("D", StopType.SetDownOnly)]
        [InlineData("U", StopType.PickUpOnly)]
        [InlineData("R", StopType.Request)]
        public void SchedulePassSetsStopTypeBasedUponAttributes(string activity, StopType expected)
        {
            var scheduleLocation = TestScheduleLocations.CreateStop(TestLocations.Surbiton, Test, activity);            
            Assert.Equal(expected, scheduleLocation.AdvertisedStop);
        }

        [Fact]
        public void SchedulePassSetsStopTypeWhenMultipleAttributes()
        {
            var scheduleLocation =  new ScheduleStop()
            {
                Location = TestLocations.Surbiton,
                Sequence = 1,
                Activities = new HashSet<string>(new []
                {
                    "-U",
                    "T"
                })
            };
            scheduleLocation.UpdateAdvertisedStop();
            Assert.Equal(StopType.Normal, scheduleLocation.AdvertisedStop);
        }     
    }
}