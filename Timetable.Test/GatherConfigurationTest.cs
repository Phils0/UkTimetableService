using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class GatherConfigurationTest
    {
        [Fact]
        public void AlwaysReturnOneResult()
        {
            var config = new GatherConfiguration(0, 0, false, GatherFilterFactory.NoFilter);
            Assert.Equal(1, config.ServicesAfter);
        }
    }
}