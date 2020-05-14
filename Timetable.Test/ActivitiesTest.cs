using Xunit;

namespace Timetable.Test
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
            var activities = Activities.Split(input);

            foreach (var expected in expectedValues)
            {
                Assert.Contains(expected, activities);
            }
        }

        [Fact]
        public void HandleNoActivities()
        {
            var activities = Activities.Split("");
            Assert.Empty(activities);
        }
        
        [Theory]
        [InlineData("T", PublicStop.Yes)]
        [InlineData("TF", PublicStop.SetDownOnly)]
        [InlineData("TB", PublicStop.PickUpOnly)]
        [InlineData("D", PublicStop.SetDownOnly)]
        [InlineData("U", PublicStop.PickUpOnly)]
        [InlineData("R", PublicStop.Request)]
        public void SchedulePassSetsStopTypeBasedUponAttributes(string activity, PublicStop expected)
        {
            var activities = new Activities(activity);            
            Assert.Equal(expected, activities.AdvertisedStop);
        }

        [Fact]
        public void AttributesIncludesNThenNotAnAdvertisedStop()
        {
            var activities = new Activities("T N");
            Assert.Equal(PublicStop.No, activities.AdvertisedStop);
        }
        
        public static TheoryData<string, PublicStop> PrecedentData =>
            new TheoryData<string, PublicStop>()
            {
                {"T TB", PublicStop.PickUpOnly },
                {"TBT", PublicStop.PickUpOnly },
                {"T TF", PublicStop.SetDownOnly },
                {"TFT", PublicStop.SetDownOnly },
                {"T R", PublicStop.Request },
                {"R T", PublicStop.Request },
                {"T U", PublicStop.PickUpOnly },
                {"U T", PublicStop.PickUpOnly },
                {"T D", PublicStop.SetDownOnly },
                {"D T", PublicStop.SetDownOnly },
            };
        
        [Theory]
        [MemberData(nameof(PrecedentData))]
        public void PrecedenceOfActivitiesWhenSettingAdvertisedStops(string activitiesString, PublicStop expected)
        {
            var activities = new Activities(activitiesString);
            Assert.Equal(expected, activities.AdvertisedStop);
        }
        
        [Theory]
        [InlineData("T", false)]
        [InlineData("T -U", true)]
        [InlineData("-UT", true)]
        [InlineData("T -D", false)]
        [InlineData("-DT", false)]
        [InlineData("D", false)]
        [InlineData("U", false)]
        [InlineData("D -D", false)]
        [InlineData("-UU", true)]
        public void IsTrainJoin(string activity, bool expected)
        {
            var activities = new Activities(activity);            
            Assert.Equal(expected, activities.IsTrainJoin);
        }
        
        [Theory]
        [InlineData("T", false)]
        [InlineData("T -U", false)]
        [InlineData("-UT", false)]
        [InlineData("T -D", true)]
        [InlineData("-DT", true)]
        [InlineData("D", false)]
        [InlineData("U", false)]
        [InlineData("D -D", true)]
        [InlineData("-UU", false)]
        public void IsTrainSplit(string activity, bool expected)
        {
            var activities = new Activities(activity);            
            Assert.Equal(expected, activities.IsTrainSplit);
        }
        
        [Theory]
        [InlineData("T", false)]
        [InlineData("T -U", false)]
        [InlineData("TB", true)]
        [InlineData("TB-D", true)]
        [InlineData("TF", false)]
        [InlineData("TF-U", false)]
        public void IsOrigin(string activity, bool expected)
        {
            var activities = new Activities(activity);            
            Assert.Equal(expected, activities.IsOrigin);
        }
        
        [Theory]
        [InlineData("T", false)]
        [InlineData("T -U", false)]
        [InlineData("TB", false)]
        [InlineData("TB-D", false)]
        [InlineData("TF", true)]
        [InlineData("TF-U", true)]
        public void IsDestination(string activity, bool expected)
        {
            var activities = new Activities(activity);            
            Assert.Equal(expected, activities.IsDestination);
        }
    }
}