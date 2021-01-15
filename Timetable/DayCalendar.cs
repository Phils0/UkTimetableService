using System;
using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// Calendar for a single day
    /// </summary>
    /// <remarks>Used for Darwin Realtime schedules</remarks>
    public class DayCalendar : ICalendar, IComparable<DayCalendar>, IComparable<CifCalendar>
    {
        public DateTime Date { get; }

        public DayCalendar(DateTime date)
        {
            Date = date;
        }
        
        public bool IsActiveOn(DateTime date)
        {
            return Date.Equals(date.Date);
        }
        
        public int CompareTo(DayCalendar other)
        {
            if (ReferenceEquals(null, other)) return -1;

            return Date.CompareTo(other.Date);
        }
        
        public int CompareTo(CifCalendar other)
        {
            if (ReferenceEquals(null, other)) return -1;

            var compare = Date.CompareTo(other.RunsFrom);
            if (compare != 0)
                return compare;  
            
            return Date.CompareTo(other.RunsTo);
        }
    }
}