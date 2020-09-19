using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class TocFilterTest
    {
        public static TheoryData<string[], string> Tocs =>
            new TheoryData<string[], string>()
            {
                {new [] {"VT"},"VT"},
                {new [] {"vt"}, "VT"},
                {new [] {"VT", "SW"},"VT|SW"},
                {new [] {"VT", "GR", "GW"}, "VT|GR|GW"},
                {new [] {""}, ""},
                {new string[0], ""},
                {null, ""}
            };

        [Theory]
        [MemberData(nameof(Tocs))]
        public void TocsToFilterString(string[] tocs, string expected)
        {
            var filter = new TocFilter(tocs);
            Assert.Equal(expected, filter.Tocs);
        }

        public static TheoryData<string[], bool> NoFilterTocs =>
            new TheoryData<string[], bool>()
            {
                {new [] {"VT"}, false},
                {new [] {"vt"}, false},
                {new [] {"VT", "SW"}, false},
                {new [] {"VT", "GR", "GW"}, false},
                {new [] {""}, true},
                {new string[0], true},
                {null, true}
            };
        
        [Theory]
        [MemberData(nameof(NoFilterTocs))]
        public void NoFilter(string[] tocs, bool expected)
        {
            var filter = new TocFilter(tocs);
            Assert.Equal(expected, filter.NoFilter);
        }
        
        public static TheoryData<string[], Toc, bool> IsValidData =>
            new TheoryData<string[], Toc, bool>()
            {
                {new [] {"VT"}, new Toc("VT"),  true},
                {new [] {"VT"}, new Toc("GW"), false},
                {new [] {"vt"}, new Toc("VT"), true},
                {new [] {"VT", "SW"}, new Toc("VT"), true},
                {new [] {"VT", "SW"}, new Toc("SW"), true},
                {new [] {"VT", "SW"}, new Toc("GW"), false},
                {new [] {""}, new Toc("VT"), true},
                {new string[0], new Toc("VT"), true},
                {null, new Toc("VT"), true}
            };
        
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid(string[] tocs, Toc testToc, bool expected)
        {
            var filter = new TocFilter(tocs);
            Assert.Equal(expected, filter.IsValid(testToc));
        }
    }
}