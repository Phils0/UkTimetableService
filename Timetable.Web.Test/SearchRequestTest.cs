using System.Collections.Generic;
using Timetable.Web.Model;
using Xunit;

namespace Timetable.Web.Test
{
    public class SearchRequestTest
    {
        public static IEnumerable<object[]> Locations
        {
            get
            {
                yield return new object[] {"ABC", "ABC"};
                yield return new object[] {"abc", "ABC"};
                yield return new object[] {"", ""};
                yield return new object[] {null, ""};
            }
        }

        [Theory]
        [MemberData(nameof(Locations))]
        public void CanonicalLocation(string input, string expected)
        {
            var request = new SearchRequest();
            request.Location = input;
            Assert.Equal(expected, request.Location);
        }
        
        [Theory]
        [MemberData(nameof(Locations))]
        public void CanonicalComingFromGoingTo(string input, string expected)
        {
            var request = new SearchRequest();
            request.ComingFromGoingTo = input;
            Assert.Equal(expected, request.ComingFromGoingTo);
        }
    }
}