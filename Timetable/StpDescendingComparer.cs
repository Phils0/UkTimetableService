using System.Collections.Generic;

namespace Timetable
{
    internal sealed class StpDescendingComparer : IComparer<(StpIndicator indicator, ICalendar calendar)>
    {
        public int Compare((StpIndicator indicator, ICalendar calendar) x,
            (StpIndicator indicator, ICalendar calendar) y)
        {
            var compare = y.indicator.CompareTo(x.indicator);
            return compare != 0 ? compare : x.calendar.CompareTo(y.calendar);
        }
    }
}