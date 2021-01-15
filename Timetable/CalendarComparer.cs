using System;
using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// Ascending date comparer
    /// </summary>
    internal sealed class CalendarComparer : IComparer<ICalendar>
    {
        public int Compare(ICalendar x, ICalendar y)
        {
            if (ReferenceEquals(null, x) && ReferenceEquals(null, y)) return 0;
            if (ReferenceEquals(null, x)) return 1;
            if (ReferenceEquals(null, y)) return -1;
            
            if (x is DayCalendar dayX)
            {
                if (y is DayCalendar day)
                    return dayX.CompareTo(day);

                if(y is CifCalendar calendar)
                    return dayX.CompareTo(calendar);
            }

            if (y is DayCalendar dayY)
            {
                if(x is CifCalendar calendar)
                    return -dayY.CompareTo(calendar);
            }

            if (x is CifCalendar calendarX && y is CifCalendar calendarY)
                return calendarX.CompareTo(calendarY);
            
            throw new ArgumentException($"Unknown Calendar type x: {x.GetType()} and y: {y.GetType()}");
        }
    }
}