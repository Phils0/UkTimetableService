using System;
using System.Collections.Generic;
using Xunit;

namespace Timetable.Test
{
    public class StationGroupLookupTest
    {
        private static StationGroup London => new("GB@LO", new[] { "EUS", "KGX", "LST" });
        private static StationGroup Manchester => new("GB@MA", new[] { "MAN", "MCV", "MCO" }, new[] { "MAN" });

        [Fact]
        public void TryGetFindsKnownGroup()
        {
            var mapper = new StationGroupLookup(new[] { London, Manchester });

            Assert.True(mapper.TryGet("GB@LO", out var group));
            Assert.Equal("GB@LO", group.Code);
        }

        [Theory]
        [InlineData("gb@lo")]
        [InlineData("Gb@Lo")]
        [InlineData("GB@LO")]
        public void TryGetIsCaseInsensitive(string code)
        {
            var mapper = new StationGroupLookup(new[] { London });

            Assert.True(mapper.TryGet(code, out var group));
            Assert.Equal("GB@LO", group.Code);
        }

        [Fact]
        public void TryGetReturnsFalseForUnknownCode()
        {
            var mapper = new StationGroupLookup(new[] { London });

            Assert.False(mapper.TryGet("GB@ZZ", out var group));
            Assert.Null(group);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TryGetReturnsFalseForNullOrEmptyCode(string code)
        {
            var mapper = new StationGroupLookup(new[] { London });

            Assert.False(mapper.TryGet(code, out var group));
            Assert.Null(group);
        }

        [Fact]
        public void EmptyMapperAlwaysReturnsFalse()
        {
            var mapper = new StationGroupLookup(Array.Empty<StationGroup>());

            Assert.False(mapper.TryGet("GB@LO", out _));
        }

        [Fact]
        public void RejectsDuplicateCodes()
        {
            var duplicate = new StationGroup("GB@LO", new[] { "PAD" });

            Assert.Throws<ArgumentException>(() => new StationGroupLookup(new[] { London, duplicate }));
        }

        [Fact]
        public void RejectsDuplicateCodesCaseInsensitively()
        {
            var lowercase = new StationGroup("gb@lo", new[] { "PAD" });

            Assert.Throws<ArgumentException>(() => new StationGroupLookup(new[] { London, lowercase }));
        }

        [Fact]
        public void RejectsNullGroups()
        {
            Assert.Throws<ArgumentNullException>(() => new StationGroupLookup(null!));
        }
    }
}