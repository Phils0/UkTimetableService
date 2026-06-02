using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class StationGroupTest
    {
        [Fact]
        public void ConstructsWithCodeMembersAndPriorities()
        {
            var manchester = TestStations.Create("MAN");
            var group = new StationGroup("GB@MA",
                new[] { manchester, TestStations.Create("MCV"), TestStations.Create("MCO") },
                new[] { manchester });

            Assert.Equal("GB@MA", group.Code);
            Assert.Equal(3, group.Members.Count);
            Assert.Single(group.Priorities!);
            Assert.Equal(manchester, group.Priorities![0]);
        }

        [Fact]
        public void MembersAreComparedByStationIdentity()
        {
            var group = new StationGroup("GB@MA", new[] { TestStations.Create("MAN"), TestStations.Create("MCV") });

            Assert.Contains(TestStations.Create("MAN"), group.Members);
            Assert.DoesNotContain(TestStations.Create("XXX"), group.Members);
        }

        [Fact]
        public void PrioritiesAreNullWhenNotSupplied()
        {
            var group = new StationGroup("GB@MA", new[] { TestStations.Create("MAN") });

            Assert.Null(group.Priorities);
        }

        [Fact]
        public void EmptyPrioritiesCollapseToNull()
        {
            var group = new StationGroup("GB@MA", new[] { TestStations.Create("MAN") }, Array.Empty<Station>());

            Assert.Null(group.Priorities);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void RejectsMissingCode(string code)
        {
            Assert.Throws<ArgumentException>(() => new StationGroup(code, new[] { TestStations.Create("MAN") }));
        }

        [Fact]
        public void RejectsNullMembers()
        {
            Assert.Throws<ArgumentNullException>(() => new StationGroup("GB@MA", null!));
        }

        [Fact]
        public void RejectsEmptyMembers()
        {
            Assert.Throws<ArgumentException>(() => new StationGroup("GB@MA", Array.Empty<Station>()));
        }

        [Fact]
        public void AcceptsPrioritiesThatAreAllMembers()
        {
            var group = new StationGroup("GB@MA",
                new[] { TestStations.Create("MAN"), TestStations.Create("MCV"), TestStations.Create("MCO") },
                new[] { TestStations.Create("MCV"), TestStations.Create("MAN") });

            Assert.Equal(2, group.Priorities!.Count);
        }
    }
}