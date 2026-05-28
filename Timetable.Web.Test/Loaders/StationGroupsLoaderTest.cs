using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Loaders;
using Xunit;

namespace Timetable.Web.Test.Loaders
{
    public class StationGroupsLoaderTest : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();

        private string WriteTempFile(string contents)
        {
            var path = Path.Combine(Path.GetTempPath(), $"station-groups-{Guid.NewGuid():N}.json");
            File.WriteAllText(path, contents);
            _tempFiles.Add(path);
            return path;
        }

        public void Dispose()
        {
            foreach (var file in _tempFiles)
                if (File.Exists(file))
                    File.Delete(file);
        }

        private static StationGroupsLoader CreateLoader(string path, ILogger logger = null) =>
            new StationGroupsLoader(path, logger ?? Substitute.For<ILogger>());

        // ILocationData mock that resolves only the supplied CRS codes (to fresh test stations) and rejects
        // everything else. Lets each test declare exactly which stations its file should be able to resolve.
        private static ILocationData LocationsWith(params string[] knownCrsCodes)
        {
            var locations = Substitute.For<ILocationData>();
            var known = new HashSet<string>(knownCrsCodes, StringComparer.OrdinalIgnoreCase);
            locations
                .TryGetStation(default, out Arg.Any<Station>())
                .ReturnsForAnyArgs(ci =>
                {
                    var crs = (string)ci[0];
                    if (crs != null && known.Contains(crs))
                    {
                        ci[1] = TestStations.Create(crs);
                        return true;
                    }
                    ci[1] = null!;
                    return false;
                });
            return locations;
        }

        [Fact]
        public async Task LoadsGroupsFromValidFile()
        {
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@MA"", ""members"": [""MAN"", ""MCV"", ""MCO""], ""priorities"": [""MAN""] },
                { ""code"": ""GB#RE"", ""members"": [""RDG"", ""RDW""] }
            ] }");
            var locations = LocationsWith("MAN", "MCV", "MCO", "RDG", "RDW");

            var lookup = await CreateLoader(path).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@MA", out var manchester).Should().BeTrue();
            manchester.Members.Select(s => s.ThreeLetterCode).Should().BeEquivalentTo("MAN", "MCV", "MCO");
            manchester.Priorities!.Select(p => p.ThreeLetterCode).Should().Equal("MAN");

            lookup.TryGet("GB#RE", out var reading).Should().BeTrue();
            reading.Priorities.Should().BeNull();
        }

        [Fact]
        public async Task LookupIsCaseInsensitive()
        {
            var path = WriteTempFile(@"{ ""groups"": [ { ""code"": ""GB@MA"", ""members"": [""MAN""] } ] }");
            var locations = LocationsWith("MAN");

            var lookup = await CreateLoader(path).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("gb@ma", out _).Should().BeTrue();
        }

        [Fact]
        public async Task EmptyGroupsArrayLoadsNoGroups()
        {
            var path = WriteTempFile(@"{ ""groups"": [] }");

            var lookup = await CreateLoader(path).LoadAsync(LocationsWith(), CancellationToken.None);

            lookup.TryGet("GB@MA", out _).Should().BeFalse();
        }

        [Fact]
        public async Task MissingFileReturnsEmptyLookupAndLogsInformation()
        {
            var logger = Substitute.For<ILogger>();
            var path = Path.Combine(Path.GetTempPath(), $"does-not-exist-{Guid.NewGuid():N}.json");

            var lookup = await CreateLoader(path, logger).LoadAsync(LocationsWith(), CancellationToken.None);

            lookup.TryGet("GB@MA", out _).Should().BeFalse();
            logger.Received().Information(Arg.Is<string>(s => s.Contains("not found")), path);
        }

        [Fact]
        public async Task NullPathReturnsEmptyLookup()
        {
            var lookup = await CreateLoader(null).LoadAsync(LocationsWith(), CancellationToken.None);

            lookup.TryGet("GB@MA", out _).Should().BeFalse();
        }

        [Fact]
        public async Task MalformedFileReturnsEmptyLookupAndLogsError()
        {
            var logger = Substitute.For<ILogger>();
            var path = WriteTempFile(@"{ this is not valid json ");

            var lookup = await CreateLoader(path, logger).LoadAsync(LocationsWith(), CancellationToken.None);

            lookup.TryGet("GB@MA", out _).Should().BeFalse();
            logger.Received().Error(Arg.Any<Exception>(), Arg.Is<string>(s => s.Contains("malformed")), path);
        }

        [Fact]
        public async Task MissingGroupsKeyLoadsNoGroups()
        {
            // Different code path from {"groups":[]}: the JSON has no "groups" property at all.
            var path = WriteTempFile(@"{}");

            var lookup = await CreateLoader(path).LoadAsync(LocationsWith(), CancellationToken.None);

            lookup.TryGet("GB@MA", out _).Should().BeFalse();
        }

        [Fact]
        public async Task LowercaseMemberCrsStillResolves()
        {
            // Master station data is uppercase by convention. Loader normalises so a lowercase JSON entry
            // still resolves rather than appearing as a missing-CRS warning.
            var path = WriteTempFile(@"{ ""groups"": [ { ""code"": ""GB@MA"", ""members"": [""man""] } ] }");
            var locations = LocationsWith("MAN");

            var lookup = await CreateLoader(path).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@MA", out var group).Should().BeTrue();
            group.Members.Select(s => s.ThreeLetterCode).Should().BeEquivalentTo("MAN");
        }

        [Fact]
        public async Task SkipsDuplicatePriorityCrs()
        {
            var logger = Substitute.For<ILogger>();
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@MA"", ""members"": [""MAN"", ""MCV""], ""priorities"": [""MAN"", ""MAN""] }
            ] }");
            var locations = LocationsWith("MAN", "MCV");

            var lookup = await CreateLoader(path, logger).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@MA", out var group).Should().BeTrue();
            group.Priorities!.Select(p => p.ThreeLetterCode).Should().Equal("MAN");
            logger.Received().Warning(Arg.Is<string>(s => s.Contains("duplicate priority")), "MAN", "GB@MA");
        }

        [Fact]
        public async Task EmptyMemberCrsIsSkippedAndLogged()
        {
            var logger = Substitute.For<ILogger>();
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@MA"", ""members"": [""MAN"", """", ""MCV""] }
            ] }");
            var locations = LocationsWith("MAN", "MCV");

            var lookup = await CreateLoader(path, logger).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@MA", out var group).Should().BeTrue();
            group.Members.Select(s => s.ThreeLetterCode).Should().BeEquivalentTo("MAN", "MCV");
            logger.Received().Warning(Arg.Is<string>(s => s.Contains("empty member entry")), "GB@MA");
        }

        [Fact]
        public async Task EmptyPriorityCrsIsSkippedAndLogged()
        {
            var logger = Substitute.For<ILogger>();
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@MA"", ""members"": [""MAN""], ""priorities"": [""MAN"", """"] }
            ] }");
            var locations = LocationsWith("MAN");

            var lookup = await CreateLoader(path, logger).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@MA", out _).Should().BeTrue();
            logger.Received().Warning(Arg.Is<string>(s => s.Contains("empty priority entry")), "GB@MA");
        }

        [Fact]
        public async Task DuplicateCodeWhereFirstHasNoResolvableMembersDoesNotBlockSecond()
        {
            // A second valid entry sharing a code with an earlier dropped one (no resolvable members) still
            // loads, because dedup runs after member resolution rather than before.
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@LO"", ""members"": [""BOGUS""] },
                { ""code"": ""GB@LO"", ""members"": [""EUS"", ""KGX""] }
            ] }");
            var locations = LocationsWith("EUS", "KGX");

            var lookup = await CreateLoader(path).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@LO", out var group).Should().BeTrue();
            group.Members.Select(s => s.ThreeLetterCode).Should().BeEquivalentTo("EUS", "KGX");
        }

        [Fact]
        public async Task SkipsMemberCrsNotInMasterStationData()
        {
            // BBB isn't known to ILocationData. Loader skips just that member; the group survives with the rest.
            var logger = Substitute.For<ILogger>();
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@PARTIAL"", ""members"": [""AAA"", ""BBB"", ""CCC""] }
            ] }");
            var locations = LocationsWith("AAA", "CCC");

            var lookup = await CreateLoader(path, logger).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@PARTIAL", out var group).Should().BeTrue();
            group.Members.Select(s => s.ThreeLetterCode).Should().BeEquivalentTo("AAA", "CCC");
            logger.Received().Warning(
                Arg.Is<string>(s => s.Contains("not found in master station data")), "BBB", "GB@PARTIAL");
        }

        [Fact]
        public async Task SkipsGroupWhenNoMembersResolve()
        {
            // None of GB@ALL_BAD's members are in master data, so the whole group is dropped. GB@OK still loads.
            var logger = Substitute.For<ILogger>();
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@ALL_BAD"", ""members"": [""AAA"", ""BBB""] },
                { ""code"": ""GB@OK"", ""members"": [""CCC""] }
            ] }");
            var locations = LocationsWith("CCC");

            var lookup = await CreateLoader(path, logger).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@ALL_BAD", out _).Should().BeFalse();
            lookup.TryGet("GB@OK", out _).Should().BeTrue();
            logger.Received().Warning(Arg.Is<string>(s => s.Contains("none of its members")), "GB@ALL_BAD");
        }

        [Fact]
        public async Task SkipsPriorityCrsNotAmongMembersButKeepsGroup()
        {
            // ZZZ is in master station data but isn't a member of GB@BAD (JSON contract violation). Loader
            // skips just the priority; the group survives with its valid members and a null Priorities list.
            var logger = Substitute.For<ILogger>();
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@BAD"", ""members"": [""AAA"", ""BBB""], ""priorities"": [""ZZZ""] }
            ] }");
            var locations = LocationsWith("AAA", "BBB", "ZZZ");

            var lookup = await CreateLoader(path, logger).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@BAD", out var group).Should().BeTrue();
            group.Priorities.Should().BeNull();
            logger.Received().Warning(
                Arg.Is<string>(s => s.Contains("not one of its members")), "ZZZ", "GB@BAD");
        }

        [Fact]
        public async Task SkipsGroupWithNoMembersInFile()
        {
            var path = WriteTempFile(@"{ ""groups"": [ { ""code"": ""GB@EMPTY"", ""members"": [] } ] }");

            var lookup = await CreateLoader(path).LoadAsync(LocationsWith(), CancellationToken.None);

            lookup.TryGet("GB@EMPTY", out _).Should().BeFalse();
        }

        [Fact]
        public async Task SkipsDuplicateCodeKeepingTheFirst()
        {
            var logger = Substitute.For<ILogger>();
            var path = WriteTempFile(@"{ ""groups"": [
                { ""code"": ""GB@MA"", ""members"": [""MAN""] },
                { ""code"": ""GB@MA"", ""members"": [""MCV""] }
            ] }");
            var locations = LocationsWith("MAN", "MCV");

            var lookup = await CreateLoader(path, logger).LoadAsync(locations, CancellationToken.None);

            lookup.TryGet("GB@MA", out var group).Should().BeTrue();
            group.Members.Select(s => s.ThreeLetterCode).Should().BeEquivalentTo("MAN");
            logger.Received().Warning(Arg.Is<string>(s => s.Contains("duplicate")), "GB@MA");
        }
    }
}