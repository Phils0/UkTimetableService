using System;
using System.Linq;
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

        public static TheoryData<Location, int> LocationComparison =>
            new TheoryData<Location, int>()
            {
                {TestLocations.Surbiton, 4},
                {TestLocations.WaterlooMain, 0},
                {TestLocations.WaterlooWindsor, -2},
            };

        [Theory]
        [MemberData(nameof(LocationComparison))]
        public void Compare(Location other, int expected)
        {
            var test = TestLocations.WaterlooMain;
            Assert.Equal(expected, test.CompareTo(other));
            Assert.Equal(expected * -1, other.CompareTo(test));
        }
        
        public static TheoryData<Location, bool> LocationEquality =>
            new TheoryData<Location, bool>()
            {
                {TestLocations.Surbiton, false},
                {TestLocations.WaterlooMain, true},
                {TestLocations.WaterlooWindsor, false},
                {null, false}
            };
        
        [Theory]
        [MemberData(nameof(LocationEquality))]
        public void EqualityUsesTiploc(Location other, bool expected)
        {
            var test = TestLocations.WaterlooMain;
            Assert.Equal(expected, test.Equals(other));
        }
    }
}