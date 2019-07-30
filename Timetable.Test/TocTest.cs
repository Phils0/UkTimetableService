using System;
using Xunit;

namespace Timetable.Test
{
    public class TocTest
    {
        private Toc VT => new Toc()
                            {
                                Code = "VT",
                                Name = "Virgin Trains"
                            };
        
        [Fact]
        public void ToSting()
        {
            Assert.Equal("VT", VT.ToString());
        }
        
        [Theory]
        [InlineData("VT", true)]
        [InlineData("GW", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void EqualsCode(string code, bool expected)
        {
            Assert.Equal(expected, VT.Equals(code));
        }
    }
}