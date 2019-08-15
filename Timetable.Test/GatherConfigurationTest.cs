using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class GatherConfigurationTest
    {
        [Fact]
        public void AlwaysReturnOneResult()
        {
            var config = new GatherConfiguration(0, 0, GatherFilterFactory.NoFilter);
            Assert.Equal(1, config.ServicesAfter);
        }
        
        [Fact]
        public void ByDefaultTimesToUseNotSet()
        {
            var config = new GatherConfiguration(0, 0, GatherFilterFactory.NoFilter);
            Assert.Equal(TimesToUse.NotSet, config.TimesToUse);
        }
    }
}