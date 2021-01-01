using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ViaRulesTest
    {
        [Fact]
        public void ChooseRightRuleBasedUponDestination()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(destination: TestLocations.WaterlooMain, text: "Correct");
            var rule2 = CreateTestRule(destination: TestLocations.Woking, text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule1);
            rules.AddRule(rule2);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }

        private static Schedule CreateTestSchedule()
        {
            return TestSchedules.CreateSchedule(stops: TestSchedules.CreateFourStopSchedule(TestSchedules.Ten));
        }
        
        private static ViaRule CreateTestRule(Station at = null, Location destination = null, Location location1 = null, Location location2 = null, string text =  "via Test")
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
        public void ChooseRightRuleBasedUponDestinationWhenAddedSecond()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(destination: TestLocations.WaterlooMain, text: "Correct");
            var rule2 = CreateTestRule(destination: TestLocations.Woking, text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule2);
            rules.AddRule(rule1);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }
        
        [Fact]
        public void ChooseRightRuleBasedUponLocation1()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(location1: TestLocations.Wimbledon, text: "Correct");
            var rule2 = CreateTestRule(location1: TestLocations.Woking, text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule1);
            rules.AddRule(rule2);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }
        
        [Fact]
        public void ChooseRightRuleBasedUponLocation1WhenAddedSecond()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(location1: TestLocations.Wimbledon, text: "Correct");
            var rule2 = CreateTestRule(location1: TestLocations.Woking, text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule2);
            rules.AddRule(rule1);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }
        
        [Fact]
        public void ChooseRightRuleBasedUponLocation2()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(location2: TestLocations.CLPHMJN, text: "Correct");
            var rule2 = CreateTestRule(location2: TestLocations.CLPHMJC, text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule1);
            rules.AddRule(rule2);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }
        
        [Fact]
        public void ChooseRightRuleBasedUponLocation2WhenAddedSecond()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(location2: TestLocations.CLPHMJN, text: "Correct");
            var rule2 = CreateTestRule(location2: TestLocations.CLPHMJC, text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule2);
            rules.AddRule(rule1);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }
        
        [Fact]
        public void ChooseRuleWithLocation2BeforeRuleWithNoLocation2()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(location2: TestLocations.CLPHMJN, text: "Correct");
            var rule2 = CreateTestRule(text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule1);
            rules.AddRule(rule2);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }
        
        [Fact]
        public void ChooseRuleWithLocation2BeforeRuleWithNoLocation2WhenAddedSecond()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(location2: TestLocations.CLPHMJN, text: "Correct");
            var rule2 = CreateTestRule(text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule2);
            rules.AddRule(rule1);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }
        
        [Fact]
        public void ChooseRuleWithNoLocation2WhenLocation2DoesNotMatch()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(location2: TestLocations.CLPHMJC, text: "Wrong");
            var rule2 = CreateTestRule(text: "Correct");

            var rules = new ViaRules();
            rules.AddRule(rule1);
            rules.AddRule(rule2);

            Assert.Equal("Correct", rules.GetViaText(schedule));
        }
        
        [Fact]
        public void ReturnEmptyStringWhenNoRulesMatch()
        {
            var schedule = CreateTestSchedule();
            var rule1 = CreateTestRule(location1: TestLocations.Woking, text: "Wrong");
            var rule2 = CreateTestRule(destination: TestLocations.Woking, text: "Wrong");

            var rules = new ViaRules();
            rules.AddRule(rule1);
            rules.AddRule(rule2);

            Assert.Equal("", rules.GetViaText(schedule));
        }
    }
}