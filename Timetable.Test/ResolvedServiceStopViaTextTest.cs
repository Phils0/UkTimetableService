using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceStopViaTextTest
    {
        private static ResolvedService CreateViaTextTestSchedule()
        {
            return TestSchedules.CreateService(stops: TestSchedules.CreateFourStopSchedule(TestSchedules.Ten));
        }

        [Fact]
        public void GoesToSetsViaTextToEmptyWhenNoViaRules206144637()
        {
            var service = CreateViaTextTestSchedule();
            var surbiton = service.Details.Locations[0];
            var stop = new ResolvedServiceStop(service, surbiton);
            
            Assert.Equal(String.Empty, stop.ViaText);
        }

        public static ViaRule CreateTestRule(Station at = null, Location destination = null, Location location1 = null, Location location2 = null, string text =  "has via text")
        {
            var rule = new ViaRule()
            {
                At = at ?? TestStations.Surbiton,
                Destination = destination ?? TestLocations.WaterlooMain,
                Location1 = location1 ?? TestLocations.Wimbledon,
                Location2 = location2 ?? Location.NotSet,
                Text = text
            };
            return rule;
        }

        [Fact]
        public void GoesToSetsViaTextWhenValidViaRule111740840()
        {
            var service = CreateViaTextTestSchedule();
            var surbiton = service.Details.Locations[0];
            surbiton.Station.Add(CreateTestRule());
            var stop = new ResolvedServiceStop(service, surbiton);
            
            Assert.Equal("has via text", stop.ViaText);
        }
        
        [Fact]
        public void GoesToSetsViaTextWhenNoValidViaRule660668605()
        {
            var service = CreateViaTextTestSchedule();
            var surbiton = service.Details.Locations[0];
            surbiton.Station.Add(CreateTestRule(destination: TestLocations.Woking));
            var stop = new ResolvedServiceStop(service, surbiton);
            
            Assert.Equal(String.Empty, stop.ViaText);
        }
    }
}