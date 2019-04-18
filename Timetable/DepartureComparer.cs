using System.Collections.Generic;

namespace Timetable
{
    public interface IDeparture
    {
        Time Departure { get; }
        Time WorkingDeparture { get; }
        /// <summary>
        /// Unique Id to allow deterministic ordering
        /// </summary>
        int Id { get; }
    }
    
    public sealed class DepartureComparer : IComparer<IDeparture>
    {
        private readonly IComparer<Time> _timeComparer = Time.TimeOfDayComparer;
            
        public int Compare(IDeparture x, IDeparture y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            
            var xDeparture = x.Departure.IsValid ? x.Departure : x.WorkingDeparture;
            var yDeparture = y.Departure.IsValid ? y.Departure : y.WorkingDeparture;
            
            var compare = _timeComparer.Compare(xDeparture, yDeparture);
            return compare != 0 ? compare : x.Id.CompareTo(y.Id);
        }
    }
}