using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ActivitiesTest
    {
        public static TheoryData<string, string[]> TestActivities =>
            new TheoryData<string, string[]>()
            {
                {"TB", new [] {"TB"}},
                {"T", new [] {"T"}},
                {"-U", new [] {"-U"}},
                {"TFN", new [] {"TF", "N"}},
                {"TFRM", new [] {"TF", "RM"}},
                {"T -DK X", new [] {"T", "-D", "K", "X"}},
            };
        
        [Theory]
        [MemberData(nameof(TestActivities))]
        public void SplitActivities(string input, string[] expectedValues)
        {
            var activities = Timetable.Web.Mapping.Activities.Split(input);

            foreach (var expected in expectedValues)
            {
                Assert.Contains(expected, activities);
            }
        }

        [Fact]
        public void HandleNoActivities()
        {
            var activities = Timetable.Web.Mapping.Activities.Split("");
            Assert.Empty(activities);
        }
    }
}