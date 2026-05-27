using System;
using Xunit;

namespace Timetable.Test
{
    public class StationGroupTest
    {
        [Fact]
        public void ConstructsWithCodeMembersAndPriorities()
        {
            var group = new StationGroup("GB@MA", new[] { "MAN", "MCV", "MCO" }, new[] { "MAN" });

            Assert.Equal("GB@MA", group.Code);
            Assert.Equal(3, group.Members.Count);
            Assert.Single(group.Priorities!);
            Assert.Equal("MAN", group.Priorities![0]);
        }

        [Fact]
        public void MembersAreCaseInsensitive()
        {
            var group = new StationGroup("GB@MA", new[] { "MAN", "MCV" });

            Assert.Contains("man", group.Members);
            Assert.Contains("MCV", group.Members);
            Assert.DoesNotContain("XXX", group.Members);
        }

        [Fact]
        public void PrioritiesAreNullWhenNotSupplied()
        {
            var group = new StationGroup("GB@MA", new[] { "MAN" });

            Assert.Null(group.Priorities);
        }

        [Fact]
        public void EmptyPrioritiesCollapseToNull()
        {
            var group = new StationGroup("GB@MA", new[] { "MAN" }, Array.Empty<string>());

            Assert.Null(group.Priorities);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void RejectsMissingCode(string code)
        {
            Assert.Throws<ArgumentException>(() => new StationGroup(code, new[] { "MAN" }));
        }

        [Fact]
        public void RejectsNullMembers()
        {
            Assert.Throws<ArgumentNullException>(() => new StationGroup("GB@MA", null!));
        }

        [Fact]
        public void RejectsEmptyMembers()
        {
            Assert.Throws<ArgumentException>(() => new StationGroup("GB@MA", Array.Empty<string>()));
        }

        [Fact]
        public void RejectsPriorityThatIsNotAMember()
        {
            Assert.Throws<ArgumentException>(() =>
                new StationGroup("GB@MA", new[] { "MAN", "MCV" }, new[] { "MCO" }));
        }

        [Fact]
        public void AcceptsPrioritiesThatAreAllMembers()
        {
            var group = new StationGroup("GB@MA", new[] { "MAN", "MCV", "MCO" }, new[] { "MCV", "man" });

            Assert.Equal(2, group.Priorities!.Count);
        }
    }
}