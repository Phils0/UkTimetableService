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
        
        public static TheoryData<ScheduleLocation, string> Locations =>
            new TheoryData<ScheduleLocation, string>()
            {
                {TestScheduleLocations.CreateOrigin(TestStations.Surbiton, Test), "00:12 SUR-SURBITN" },
                {TestScheduleLocations.CreateDestination(TestStations.Surbiton, Test), "00:12 SUR-SURBITN" },
                {TestScheduleLocations.CreatePass(TestStations.Surbiton, Test), "00:12 SUR-SURBITN" },
                {TestScheduleLocations.CreateStop(TestStations.Surbiton, Test), "00:12 SUR-SURBITN" },
                {TestScheduleLocations.CreateStop(TestStations.Surbiton, Test, sequence: 2), "00:12 SUR-SURBITN+2" },
                {TestScheduleLocations.CreatePickupOnlyStop(TestStations.Surbiton, Test), "00:12 SUR-SURBITN" },
            };
        
        [Theory]
        [MemberData(nameof(Locations))]
        public void ToStringOutputsTimePlusLocation(ScheduleLocation location, string expected)
        {
            Assert.Equal(expected, location.ToString());
        }
        
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
            var scheduleLocation = TestScheduleLocations.CreateDestination(TestStations.Surbiton, Test);
            scheduleLocation.AddDay(Start);
            
            Assert.Equal(Expected, scheduleLocation.Arrival);
            Assert.Equal(Expected.Subtract(ThirtySeconds), scheduleLocation.WorkingArrival);
        }
        
        [Fact]
        public void AddDayToSchedulePass()
        {
            var scheduleLocation = TestScheduleLocations.CreatePass(TestStations.Surbiton, Test);
            scheduleLocation.AddDay(Start);
            
            Assert.Equal(Expected, scheduleLocation.PassesAt);
        }

        [Fact]
        public void ScheduleOriginStopTypeIsOrigin()
        {
            var scheduleLocation = TestScheduleLocations.CreateOrigin(TestStations.Surbiton, Test);
            Assert.Equal(PublicStop.PickUpOnly, scheduleLocation.AdvertisedStop);
        }
        
        [Fact]
        public void ScheduleDestinationStopTypeIsDestination()
        {
            var scheduleLocation = TestScheduleLocations.CreateDestination(TestStations.Surbiton, Test);
            Assert.Equal(PublicStop.SetDownOnly, scheduleLocation.AdvertisedStop);
        }
        
        [Fact]
        public void SchedulePassIsNotAStop()
        {
            var scheduleLocation = TestScheduleLocations.CreatePass(TestStations.Surbiton, Test);            
            Assert.Equal(PublicStop.No, scheduleLocation.AdvertisedStop);
        }
        
        [Theory]
        [InlineData("T", PublicStop.Yes)]
        [InlineData("D", PublicStop.SetDownOnly)]
        [InlineData("U", PublicStop.PickUpOnly)]
        [InlineData("R", PublicStop.Request)]
        public void SchedulePassSetsStopTypeBasedUponAttributes(string activity, PublicStop expected)
        {
            var scheduleLocation = TestScheduleLocations.CreateStop(TestStations.Surbiton, Test, activity);            
            Assert.Equal(expected, scheduleLocation.AdvertisedStop);
        }

        [Fact]
        public void AttributesIncludesNThenNotAnAdvertisedStop()
        {
            var scheduleLocation = TestScheduleLocations.CreateStop(TestStations.Surbiton, Test, "T"); 
            scheduleLocation.Activities = new HashSet<string>(new [] {"T", "N"});
            scheduleLocation.UpdateAdvertisedStop();
            Assert.Equal(PublicStop.No, scheduleLocation.AdvertisedStop);
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
            Assert.Equal(PublicStop.Yes, scheduleLocation.AdvertisedStop);
        }

        public static TheoryData<String[], PublicStop> PrecedentData =>
            new TheoryData<String[], PublicStop>()
            {
                {new [] {"T", "TB"}, PublicStop.PickUpOnly },
                {new [] {"TB", "T"}, PublicStop.PickUpOnly },
                {new [] {"T", "TF"}, PublicStop.SetDownOnly },
                {new [] {"TF", "T"}, PublicStop.SetDownOnly },
                {new [] {"T", "R"}, PublicStop.Request },
                {new [] {"R", "T"}, PublicStop.Request },
                {new [] {"T", "U"}, PublicStop.PickUpOnly },
                {new [] {"U", "T"}, PublicStop.PickUpOnly },
                {new [] {"T", "D"}, PublicStop.SetDownOnly },
                {new [] {"D", "T"}, PublicStop.SetDownOnly },
            };
        
        [Theory]
        [MemberData(nameof(PrecedentData))]
        public void PrecedenceOfActivitiesWhenSettingAdvertisedStops(string[] activities, PublicStop expected)
        {
            var scheduleLocation =  new ScheduleStop()
            {
                Location = TestLocations.Surbiton,
                Sequence = 1,
                Activities = new HashSet<string>(activities)
            };
            scheduleLocation.UpdateAdvertisedStop();
            Assert.Equal(expected, scheduleLocation.AdvertisedStop);
        }
    }
}