using System.Collections.Generic;

namespace Timetable
{
    public interface IArrival
    {
        Time Arrival { get; }
        Time WorkingArrival { get; }       
        /// <summary>
        /// Unique Id to allow deterministic ordering
        /// </summary>
        int Id { get; }
    }
    
    public sealed class ArrivalComparer : IComparer<IArrival>
    {
        private readonly IComparer<Time> _timeComparer = Time.TimeOfDayComparer;
            
        public int Compare(IArrival x, IArrival y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            var xArrival = x.Arrival.IsValid ? x.Arrival : x.WorkingArrival;
            var yArrival = y.Arrival.IsValid ? y.Arrival : y.WorkingArrival;
            
            var compare = _timeComparer.Compare(yArrival, xArrival);    // Descending order
            return compare != 0 ? compare : x.Id.CompareTo(y.Id);
        }
    }
}