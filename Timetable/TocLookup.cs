using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    public class TocLookup : IEnumerable<Toc>, ILookup<string, Toc>
    {
        private readonly ConcurrentDictionary<string, Toc> _values;
        private readonly ILogger _logger;
        
        public TocLookup(ILogger logger, Dictionary<string, Toc> data)
        {
            _logger = logger;
            _values = new ConcurrentDictionary<string, Toc>(data);
        }

        public Toc FindOrAdd(string key)
        {
            if (!_values.TryGetValue(key, out var value))
            {
                _logger.Information("{key} Toc not found. Creating.", key);
                var newValue = new Toc(key);
                value = _values.GetOrAdd(key, newValue);
            }

            return value;
        }
        
        public void Add(string key, Toc toc)
        {
            if (!_values.TryAdd(key, toc))
            {
                _logger.Error("Duplicate Toc {key}.", key);
            }
        }

        IEnumerator<IGrouping<string, Toc>> IEnumerable<IGrouping<string, Toc>>.GetEnumerator()
        {
            return _values
                .GroupBy(k => k.Key, k => k.Value)
                .GetEnumerator();
        }

        public IEnumerator<Toc> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(string key)
        {
            return _values.ContainsKey(key);
        }

        public int Count => _values.Count;

        public IEnumerable<Toc> this[string key] =>  new [] { _values[key] };
    }
}