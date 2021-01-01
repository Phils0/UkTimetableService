using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ViaRuleTest
    {
        [Fact]
        public void DestinationAndLocation1MatchThenIsMatch()
        {
            var schedule = CreateTestSchedule();
            var rule = CreateTestRule();
            
            Assert.True(rule.IsSatisfied(schedule));
        }

        private static Schedule CreateTestSchedule()
        {
            return TestSchedules.CreateSchedule(stops: TestSchedules.CreateFourStopSchedule(TestSchedules.Ten));
        }

        private static ViaRule CreateTestRule(Station at = null, Location destination = null, Location location1 = null, Location location2 = null)
        {
            var rule = new ViaRule()
            {
                At = at ?? TestStations.Surbiton,
                Destination = destination ?? TestLocations.WaterlooMain,
                Location1 = location1 ?? TestLocations.CLPHMJN,
                Location2 = location2 ?? Location.NotSet,
                Text = "via Test"
            };
            return rule;
        }

        [Fact]
        public void DestinationAndLocation1AndLocation2MatchThenIsMatch()
        {
            var schedule = CreateTestSchedule();
            var rule = CreateTestRule(location1: TestLocations.Wimbledon, location2: TestLocations.CLPHMJN);

            Assert.True(rule.IsSatisfied(schedule));
        }

        [Fact]
        public void DestinationDoesNotMatch()
        {
            var schedule = CreateTestSchedule();
            var rule = CreateTestRule(destination: TestLocations.Woking);
            
            Assert.False(rule.IsSatisfied(schedule));
        }
        
        [Fact]
        public void DestinationMatchesButLocation1DoesNotMatch()
        {
            var schedule = CreateTestSchedule();
            var rule = CreateTestRule(location1: TestLocations.Woking);
            
            Assert.False(rule.IsSatisfied(schedule));           
        }
        
        [Fact]
        public void DestinationAndLocation1MatchButLocation2DoesNotMatch()
        {
            var schedule = CreateTestSchedule();
            var rule = CreateTestRule(location2: TestLocations.Woking);
            
            Assert.False(rule.IsSatisfied(schedule)); 
        }
        
        [Fact]
        public void DestinationMatchesButLocation1AndLocation2InWrongOrder()
        {            
            var schedule = CreateTestSchedule();
            var rule = CreateTestRule(location1: TestLocations.CLPHMJN, location2: TestLocations.Wimbledon);

            Assert.False(rule.IsSatisfied(schedule));
        }
        
        [Fact]
        public void DestinationMatchesButLocation1BeforeMainLocation()
        {
            var schedule = CreateTestSchedule();
            var rule = CreateTestRule(at: TestStations.ClaphamJunction, location1: TestLocations.Surbiton);
            
            Assert.False(rule.IsSatisfied(schedule));             
        }
        
        [Fact]
        public void RuleDestinationIsNotScheduleDestinationDoesNotMatch()
        {
            var schedule = CreateTestSchedule();
            var rule = CreateTestRule(destination: TestLocations.CLPHMJN, location1: TestLocations.Wimbledon);

            Assert.False(rule.IsSatisfied(schedule));
        }
        
        [Fact]
        public void HasLocation2()
        {
            var rule = CreateTestRule(location2: TestLocations.Wimbledon);
            
            Assert.True(rule.HasLocation2);
        }
        
        [Fact]
        public void DoesNotHaveLocation2()
        {
            var rule = CreateTestRule();
            
            Assert.False(rule.HasLocation2);
        }
        
        [Fact]
        public void ToStringWithLocation2()
        {
            var rule = CreateTestRule(location2: TestLocations.Wimbledon);
            
            Assert.Equal("SUR To:WATRLMN Loc1-CLPHMJN Loc2-WIMBLDN", rule.ToString());
        }
        
        [Fact]
        public void ToStringNoLocation2()
        {
            var rule = CreateTestRule();
            
            Assert.Equal("SUR To:WATRLMN Loc1-CLPHMJN", rule.ToString());
        }
    }
}