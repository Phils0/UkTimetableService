using Timetable.Web.Mapping.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
{
    public class ActivitiesTest
    {
        public static TheoryData<string> TestActivities =>
            new TheoryData<string>()
            {
                "TB",
                "T",
                "-U",
                "TFN",
                "TFRM",
                "T -DK X",
                ""
            };
        
        [Theory]
        [MemberData(nameof(TestActivities))]
        public void SplitActivities(string input)
        {
            var converter = new ActivitiesConverter();
            var activities = converter.Convert(input, null);

            Assert.Equal(input, activities.Value);
        }

        [Fact]
        public void ReturnsSameValueForSameSetOfActivities()
        {
            var converter = new ActivitiesConverter();
            var activities = converter.Convert("TB", null);
            
            var same = converter.Convert("TB", null);
            Assert.Same(activities, same);
            
            var different = converter.Convert("TB-D", null);
            Assert.NotSame(activities, different);
        }
    }
}