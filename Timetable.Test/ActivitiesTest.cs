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
    }
}