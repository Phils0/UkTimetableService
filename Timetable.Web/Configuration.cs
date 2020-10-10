using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Timetable.Web
{
    internal class Configuration
    {
        private readonly IConfiguration _config;

        internal Configuration(IConfiguration config)
        {
            _config = config;
        }

        internal bool EnableCustomPlugins => IsTrue(_config["EnableCustomPlugins"]);
        internal bool EnablePrometheusMonitoring => IsTrue(_config["EnablePrometheusMonitoring"]);

        private bool IsTrue(string val)
        {
            return !string.IsNullOrEmpty(val) && val.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }
        
        public string TimetableArchiveFile
        {
            get
            {
                var path = Path.Combine(@"Data", _config["TimetableArchive"]);
                var file = new FileInfo(path);
                return file.FullName;
            }
        }
        
        public string StationsKnowledgebaseFile
        {
            get
            {
                var path = Path.Combine(@"Data", _config["StationKnowledgebase"]);
                var file = new FileInfo(path);
                return file.FullName;
            }
        }
        
        public string TocsKnowledgebaseFile
        {
            get
            {
                var path = Path.Combine(@"Data", _config["TocKnowledgebase"]);
                var file = new FileInfo(path);
                return file.FullName;
            }
        }
    }
}