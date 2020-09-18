using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Serilog;

namespace Timetable
{
    public class Toc : IEquatable<string>, IEquatable<Toc>
    {
        public static readonly Toc Unknown = new Toc()
        {
            Code = "??",
            Name = "Unknown"
        };

        public string Code { get; set; }

        public string Name { get; set; } = "";

        public bool Equals(string other)
        {
            return Code.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Toc) obj);
        }

        public bool Equals(Toc other)
        {
            return Code == other.Code;
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Code;
        }
    }

    public class TocLookup
    {
        private readonly ConcurrentDictionary<string, Toc> _values;
        private readonly ILogger _logger;

        
        public TocLookup(ILogger logger, Dictionary<string, Toc> data)
        {
            _logger = logger;
            _values = new ConcurrentDictionary<string, Toc>(data);
        }

        public Toc Find(string key)
        {
            if (!_values.TryGetValue(key, out var value))
            {
                _logger.Information("{key} Toc not found. Creating.", key);
                var newValue = new Toc()
                {
                    Code = key
                };
                value = _values.GetOrAdd(key, newValue);
            }

            return value;
        }
    }
}