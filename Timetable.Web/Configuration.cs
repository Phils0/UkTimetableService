using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Timetable.Web
{
    internal class Configuration
    {
        public const string DataDirectory = @"Data";
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        internal Configuration(IConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        internal bool EnableCustomPlugins => IsTrue(_config["EnableCustomPlugins"], "EnableCustomPlugins");
        internal bool EnablePrometheusMonitoring => IsTrue(_config["EnablePrometheusMonitoring"], "EnablePrometheusMonitoring");

        private bool IsTrue(string val, string name)
        {
            _logger.Debug("Config {name}: {val}", name, val);
            return !string.IsNullOrEmpty(val) && val.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }
        
        internal bool EnableDebugResponses => IsTrue(_config["EnableDebugResponses"], "EnableDebugResponses");

        public string TimetableArchiveFile
        {
            get
            {
                var path = Path.Combine(DataDirectory, _config["TimetableArchive"]);
                _logger.Debug("Config TimetableArchive: {path}", path);
                var file = new FileInfo(path);
                return file.FullName;
            }
        }
        
        public string StationsKnowledgebaseFile
        {
            get
            {
                var path = Path.Combine(DataDirectory, _config["StationKnowledgebase"]);
                _logger.Debug("Config StationKnowledgebase: {path}", path);
                var file = new FileInfo(path);
                return file.FullName;
            }
        }
        
        public string TocsKnowledgebaseFile
        {
            get
            {
                var path = Path.Combine(DataDirectory, _config["TocKnowledgebase"]);
                _logger.Debug("Config TocKnowledgebase: {path}", path);
                var file = new FileInfo(path);
                return file.FullName;
            }
        }

        public DateTime? DarwinDate
        {
            get
            {
                var configValue = _config["DarwinDate"];
                _logger.Debug("Config DarwinDate: {configValue}", configValue);
                if (!string.IsNullOrEmpty(configValue) && DateTime.TryParse(configValue, out var value))
                    return value;
                
                _logger.Information("No Darwin Date, will pick latest.  Original value {configValue}", configValue);
                return null;
            }
        }

        /// <summary>
        /// Full path to the optional station-groups reference file. Null when the key is unset, in which case
        /// station group search is disabled (the loader treats a missing path the same as a missing file).
        /// </summary>
        public string? StationGroupsFile
        {
            get
            {
                var fileName = _config["StationGroupsFile"];
                
                if (string.IsNullOrEmpty(fileName))
                {
                    _logger.Information("Config StationGroupsFile: <not set>");
                    return null;
                }

                var path = Path.Combine(DataDirectory, fileName);
                _logger.Debug("Config StationGroupsFile: {path}", path);
                
                return new FileInfo(path).FullName;
            }
        }

        /// <summary>
        /// The journey heuristic the station-group optimiser uses when a service calls at several members of a
        /// group. Defaults to <see cref="JourneyHeuristic.Longest"/> when unset or unrecognised.
        /// </summary>
        public JourneyHeuristic StationGroupOptimisationStrategy
        {
            get
            {
                var configValue = _config["StationGroupOptimisationStrategy"];
                
                // Enum.TryParse alone accepts integers outside the defined range (e.g. "42" becomes
                // (JourneyHeuristic)42, which silently falls into the Shortest branch in the optimiser).
                // Guard with Enum.IsDefined so only named values pass through.
                if (!string.IsNullOrEmpty(configValue) &&
                    Enum.TryParse<JourneyHeuristic>(configValue, ignoreCase: true, out var heuristic) &&
                    Enum.IsDefined(typeof(JourneyHeuristic), heuristic)) {
                    _logger.Debug("Config StationGroupOptimisationStrategy: {configValue}", configValue);
                    return heuristic;
                }

                if (string.IsNullOrEmpty(configValue))
                    _logger.Information(
                        "StationGroupOptimisationStrategy not set; defaulting to {Default}",
                        JourneyHeuristic.Longest);
                else
                    _logger.Warning(
                        "StationGroupOptimisationStrategy '{configValue}' not recognised; defaulting to {Default}",
                        configValue, JourneyHeuristic.Longest);

                return JourneyHeuristic.Longest;
            }
        }
    }
}