using System;
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

        [Fact]
        public void TryGetFindsKnownGroup()
        {
            var lookup = new StationGroupLookup(new[] { London, Manchester });

            Assert.True(lookup.TryGet("GB@LO", out var group));
            Assert.Equal("GB@LO", group.Code);
        }

        [Theory]
        [InlineData("gb@lo")]
        [InlineData("Gb@Lo")]
        [InlineData("GB@LO")]
        public void TryGetIsCaseInsensitive(string code)
        {
            var lookup = new StationGroupLookup(new[] { London });

            Assert.True(lookup.TryGet(code, out var group));
            Assert.Equal("GB@LO", group.Code);
        }

        [Fact]
        public void TryGetReturnsFalseForUnknownCode()
        {
            var lookup = new StationGroupLookup(new[] { London });

            Assert.False(lookup.TryGet("GB@ZZ", out var group));
            Assert.Null(group);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TryGetReturnsFalseForNullOrEmptyCode(string code)
        {
            var lookup = new StationGroupLookup(new[] { London });

            Assert.False(lookup.TryGet(code, out var group));
            Assert.Null(group);
        }

        [Fact]
        public void EmptyLookupAlwaysReturnsFalse()
        {
            var lookup = new StationGroupLookup(Array.Empty<StationGroup>());

            Assert.False(lookup.TryGet("GB@LO", out _));
        }

        [Fact]
        public void RejectsDuplicateCodes()
        {
            var duplicate = new StationGroup("GB@LO", new[] { TestStations.Create("PAD") });

            Assert.Throws<ArgumentException>(() => new StationGroupLookup(new[] { London, duplicate }));
        }

        [Fact]
        public void RejectsDuplicateCodesCaseInsensitively()
        {
            var lowercase = new StationGroup("gb@lo", new[] { TestStations.Create("PAD") });

            Assert.Throws<ArgumentException>(() => new StationGroupLookup(new[] { London, lowercase }));
        }

        [Fact]
        public void RejectsNullGroups()
        {
            Assert.Throws<ArgumentNullException>(() => new StationGroupLookup(null!));
        }
    }
}