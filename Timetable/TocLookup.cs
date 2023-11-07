using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    public interface ITocLookup : ILookup<string, Toc>
    {
        IEnumerable<Toc> AsEnumerableToc();
    }
    
    public class TocLookup : ITocLookup
    {
        private readonly ConcurrentDictionary<string, Toc> _values;
        private readonly ILogger _logger;
        
        public TocLookup(ILogger logger, Dictionary<string, Toc> data = null)
        {
            _logger = logger;
            _values = data == null ?
                new ConcurrentDictionary<string, Toc>() : 
                new ConcurrentDictionary<string, Toc>(data);
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
        
        public void AddIfNotExist(string key, Toc toc)
        {
            if (!_values.TryAdd(key, toc))
            {
                _logger.Information("Duplicate Toc {key} new value not set {@toc}.", key, toc);
            }
        }
        
        public void AddOrReplace(string key, Toc toc)
        {
            if (!_values.TryAdd(key, toc))
            {
                _values[key] = toc;
                _logger.Information("Replace Toc {key}.", key);
            }
        }

        IEnumerator<IGrouping<string, Toc>> IEnumerable<IGrouping<string, Toc>>.GetEnumerator()
        {
            return _values
                .GroupBy(k => k.Key, k => k.Value)
                .GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return AsEnumerableToc().GetEnumerator();
        }

        public bool Contains(string key)
        {
            return _values.ContainsKey(key);
        }

        public int Count => _values.Count;

        public IEnumerable<Toc> this[string key] =>  new [] { _values[key] };
        
        /// <summary>
        /// Provide tocs as a <see cref="IEnumerable{Toc}" />
        /// </summary>
        /// <remarks>Needed as <see cref="ILookup{TKey,TElement}" returns a clashing <see cref="IEnumerable{T}"/> definition/></remarks>
        public IEnumerable<Toc> AsEnumerableToc()
        {
            return _values.Values;
        }
    }
}