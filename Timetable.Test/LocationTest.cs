using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class LocationTest
    {       
        [Fact]
        public void ToStringForNotSetReturnsNotSet()
        {
            Assert.Equal("Not Set", Location.NotSet.ToString());
        }
                
        [Fact]
        public void ToStringReturnsCrsAndTiploc()
        {
            Assert.Equal("WAT-WATRLMN", TestLocations.WaterlooMain.ToString());
            Assert.Equal("WAT-WATRLOW", TestLocations.WaterlooWindsor.ToString());
        }
    }
}