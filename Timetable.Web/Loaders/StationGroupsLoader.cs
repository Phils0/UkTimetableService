using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Timetable.Web.Loaders
{
    /// <inheritdoc />
    /// <remarks>
    /// Reads a small vendor-neutral JSON file of the shape
    /// <c>{ "groups": [ { "code": "GB@LO", "members": ["EUS","KGX"], "priorities": ["EUS"] } ] }</c>.
    /// All failure modes degrade gracefully (consistent with the other optional reference-data loaders):
    /// an absent or malformed file disables the feature with a single log line, and a single invalid group,
    /// member or priority is skipped with a warning rather than taking the rest of the file down. A summary
    /// line at the end of a successful load reports the totals so partial-deploy issues are alertable.
    /// </remarks>
    public class StationGroupsLoader : IStationGroupsLoader
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string? _filePath;
        private readonly ILogger _logger;

        public StationGroupsLoader(string? filePath, ILogger logger)
        {
            _filePath = filePath;
            _logger = logger;
        }

        public async Task<StationGroupLookup> LoadAsync(ILocationData locations, CancellationToken token)
        {
            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
            {
                _logger.Information("station-groups file not found at {Path} - station group search disabled", _filePath);
                return Empty();
            }

            var file = await ReadFileAsync(_filePath, token).ConfigureAwait(false);
            if (file == null)
                return Empty();

            var report = new LoadReport();
            var groups = BuildGroups(file.Groups, locations, report);

            _logger.Information(
                "Loaded {Count} station groups from {Path} " +
                "(skipped {GroupsSkipped} group(s), {MembersSkipped} member(s), {PrioritiesSkipped} priorities)",
                groups.Count, _filePath, report.GroupsSkipped, report.MembersSkipped, report.PrioritiesSkipped);

            return new StationGroupLookup(groups);
        }

        private async Task<StationGroupsFile?> ReadFileAsync(string path, CancellationToken token)
        {
            try
            {
                var json = await File.ReadAllTextAsync(path, token).ConfigureAwait(false);
                return JsonSerializer.Deserialize<StationGroupsFile>(json, SerializerOptions);
            }
            catch (JsonException e)
            {
                _logger.Error(e, "station-groups file at {Path} is malformed - station group search disabled", path);
                return null;
            }
        }

        private IReadOnlyList<StationGroup> BuildGroups(IReadOnlyList<StationGroupJsonDefinition>? definitions, ILocationData locations, LoadReport report)
        {
            var groups = new List<StationGroup>();
            if (definitions == null)
                return groups;

            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var definition in definitions)
            {
                var members = ResolveMembers(definition.Members, locations, definition.Code, report);
                if (members.Count == 0)
                {
                    _logger.Warning(
                        "Skipping station group {Code} - none of its members were found in master station data",
                        definition.Code);
                    report.GroupsSkipped++;
                    continue;
                }

                // Dedup AFTER member resolution: a valid second entry shouldn't be rejected as a "duplicate"
                // of an earlier entry that itself got dropped for having no resolvable members.
                if (!string.IsNullOrEmpty(definition.Code) && !seenCodes.Add(definition.Code))
                {
                    _logger.Warning("Skipping duplicate station group code {Code}", definition.Code);
                    report.GroupsSkipped++;
                    continue;
                }
                
                var priorities = definition.Priorities is { Count: > 0 }
                    ? ResolvePriorities(definition.Priorities, members, definition.Code, report)
                    : null;

                try
                {
                    groups.Add(new StationGroup(definition.Code, members, priorities));
                }
                catch (ArgumentException e)
                {
                    // Belt-and-braces: catches any remaining StationGroup ctor validation (e.g. empty code)
                    // so one bad entry is a skip, not a crash.
                    _logger.Warning(e, "Skipping invalid station group {Code}", definition.Code);
                    report.GroupsSkipped++;
                }
            }

            return groups;
        }

        // Resolves member CRS codes to Stations via the master station data. CRS not in the data, and empty
        // entries, are logged and skipped - the caller decides whether the remaining set is sufficient.
        private List<Station> ResolveMembers(
            IReadOnlyList<string>? memberCodes, ILocationData locations, string groupCode, LoadReport report)
        {
            var resolved = new List<Station>();
            if (memberCodes == null)
                return resolved;
            foreach (var crs in memberCodes)
            {
                if (string.IsNullOrEmpty(crs))
                {
                    _logger.Warning("Skipping empty member entry in station group {Code}", groupCode);
                    report.MembersSkipped++;
                    continue;
                }
                
                if (locations.TryGetStation(crs.ToUpperInvariant(), out var station))
                    resolved.Add(station);
                else
                {
                    _logger.Warning(
                        "Skipping member {Crs} of station group {Code} - not found in master station data",
                        crs, groupCode);
                    report.MembersSkipped++;
                }
            }
            return resolved;
        }

        // Resolves priority CRS codes against the already-resolved members because by contract they should be a subset.
        // A priority that isn't one of the members (or that duplicates one already added) is skipped with a warning.
        private List<Station> ResolvePriorities(
            IReadOnlyList<string> priorityCodes, IReadOnlyList<Station> members, string groupCode, LoadReport report)
        {
            var resolved = new List<Station>();
            var seen = new HashSet<Station>();
            foreach (var crs in priorityCodes)
            {
                if (string.IsNullOrEmpty(crs))
                {
                    _logger.Warning("Skipping empty priority entry in station group {Code}", groupCode);
                    report.PrioritiesSkipped++;
                    continue;
                }
                var member = members.FirstOrDefault(m =>
                    StringComparer.OrdinalIgnoreCase.Equals(m.ThreeLetterCode, crs));
                
                if (member == null)
                {
                    _logger.Warning(
                        "Skipping priority {Crs} of station group {Code} - not one of its members",
                        crs, groupCode);
                    report.PrioritiesSkipped++;
                    continue;
                }
                
                if (!seen.Add(member))
                {
                    _logger.Warning(
                        "Skipping duplicate priority {Crs} of station group {Code}",
                        crs, groupCode);
                    report.PrioritiesSkipped++;
                    continue;
                }
                resolved.Add(member);
            }
            return resolved;
        }

        private static StationGroupLookup Empty() => new StationGroupLookup(Array.Empty<StationGroup>());

        private sealed class StationGroupsFile
        {
            public List<StationGroupJsonDefinition>? Groups { get; set; }
        }

        private sealed class StationGroupJsonDefinition
        {
            public string Code { get; set; } = string.Empty;
            public List<string>? Members { get; set; }
            public List<string>? Priorities { get; set; }
        }

        private sealed class LoadReport
        {
            public int GroupsSkipped;
            public int MembersSkipped;
            public int PrioritiesSkipped;
        }
    }
}