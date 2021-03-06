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
        [InlineData("T N", PublicStop.No)]
        public void ScheduleStopSetsStopTypeBasedUponActivities(string activity, PublicStop expected)
        {
            var scheduleLocation = TestScheduleLocations.CreateStop(TestStations.Surbiton, Test, activity);            
            Assert.Equal(expected, scheduleLocation.AdvertisedStop);
        }
        
        [Theory]
        [InlineData("T-U", AssociationCategory.Join, true)]
        [InlineData("T", AssociationCategory.Join, false)]
        [InlineData("TB", AssociationCategory.Join, false)]
        [InlineData("TF", AssociationCategory.Join, false)]
        [InlineData("T-D", AssociationCategory.Split, true)]
        [InlineData("T", AssociationCategory.Split, false)]
        [InlineData("TB", AssociationCategory.Split, false)]
        [InlineData("TF", AssociationCategory.Split, false)]
        public void IsMainConsistent(string activity, AssociationCategory category, bool expected)
        {
            var scheduleLocation = TestScheduleLocations.CreateStop(TestStations.Surbiton, Test, activity);            
            Assert.Equal(expected, scheduleLocation.IsMainConsistent(category));
        }
        
        [Theory]
        [InlineData("T-U", AssociationCategory.Join, false)]
        [InlineData("T", AssociationCategory.Join, false)]
        [InlineData("TB", AssociationCategory.Join, false)]
        [InlineData("TF", AssociationCategory.Join, true)]
        [InlineData("T-D", AssociationCategory.Split, false)]
        [InlineData("T", AssociationCategory.Split, false)]
        [InlineData("TB", AssociationCategory.Split, true)]
        [InlineData("TF", AssociationCategory.Split, false)]
        public void IsAssociatedConsistent(string activity, AssociationCategory category, bool expected)
        {
            var scheduleLocation = TestScheduleLocations.CreateStop(TestStations.Surbiton, Test, activity);            
            Assert.Equal(expected, scheduleLocation.IsAssociatedConsistent(category));
        }
        
        public static TheoryData<string, bool> AdvertisedStop =>
            new TheoryData<string, bool>()
            {
                {Activities.StopNotAdvertised,false},
                {Activities.PassengerStop, true},
                {Activities.PickUpOnlyStop, true},
                {Activities.SetDownOnlyStop, true},
                {Activities.RequestStop, true},
                {Activities.TrainBegins, true},
                {Activities.TrainFinishes, true},  
            };

        [Theory]
        [MemberData(nameof(AdvertisedStop))]
        public void IsAdvertisedStop(string activity, bool expected)
        {
            var stop = TestScheduleLocations.CreateStop(TestStations.Surbiton, TestSchedules.Ten, activity);
            Assert.Equal(expected, stop.IsAdvertised());
        }
    }
}