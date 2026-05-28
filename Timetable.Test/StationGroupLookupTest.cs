using System;
using System.Collections.Generic;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class StationGroupLookupTest
    {
        private static StationGroup London =>
            new("GB@LO",
                new[] { TestStations.Create("EUS"), TestStations.Create("KGX"), TestStations.Create("LST") });

        private static StationGroup Manchester =>
            new("GB@MA",
                new[] { TestStations.Create("MAN"), TestStations.Create("MCV"), TestStations.Create("MCO") },
                new[] { TestStations.Create("MAN") });
        
        private static StationGroupLookup LookupOf(params StationGroup[] groups) =>
            new(groups.ToDictionary(g => g.Code, g => g, StringComparer.OrdinalIgnoreCase));

        [Fact]
        public void TryGetFindsKnownGroup()
        {
            var lookup = LookupOf(London, Manchester);

            Assert.True(lookup.TryGet("GB@LO", out var group));
            Assert.Equal("GB@LO", group.Code);
        }

        [Theory]
        [InlineData("gb@lo")]
        [InlineData("Gb@Lo")]
        [InlineData("GB@LO")]
        public void TryGetIsCaseInsensitive(string code)
        {
            var lookup = LookupOf(London);

            Assert.True(lookup.TryGet(code, out var group));
            Assert.Equal("GB@LO", group.Code);
        }

        [Fact]
        public void TryGetReturnsFalseForUnknownCode()
        {
            var lookup = LookupOf(London);

            Assert.False(lookup.TryGet("GB@ZZ", out var group));
            Assert.Null(group);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TryGetReturnsFalseForNullOrEmptyCode(string code)
        {
            var lookup = LookupOf(London);

            Assert.False(lookup.TryGet(code, out var group));
            Assert.Null(group);
        }

        [Fact]
        public void EmptyLookupAlwaysReturnsFalse()
        {
            var lookup = LookupOf();

            Assert.False(lookup.TryGet("GB@LO", out _));
        }

        [Fact]
        public void RejectsNullGroups()
        {
            Assert.Throws<ArgumentNullException>(() => new StationGroupLookup(null!));
        }
    }
}