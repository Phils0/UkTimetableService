using System.Collections.Generic;
using Serilog;

namespace Timetable
{
    public class Toc
    {
        public string Code { get; set; }

        public string Name { get; set; } = "";

        public override string ToString()
        {
            return Code;
        }
    }
    
    public class TocLookup : Dictionary<string, Toc> 
    {
        private readonly ILogger _logger;

        public TocLookup(ILogger logger)
        {
            _logger = logger;
        }
        
        public Toc Find(string key)
        {
            if (TryGetValue(key, out var value))
                return value;

            _logger.Warning("{key} Toc not found. Creating.", key);
            var newValue = new Toc()
            {
                Code = key
            };
            Add(key, newValue);
            return newValue;
        }
    }
}