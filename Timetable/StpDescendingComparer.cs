using System.Collections.Generic;

namespace Timetable
{
    /// <summary>
    /// Comparer ordered by STP indicator Cancelled, New, Override, Permanent
    /// </summary>
    /// <remarks>Calendar order is required but specifics unimportant</remarks>
    internal sealed class StpDescendingComparer : IComparer<(StpIndicator indicator, ICalendar calendar)>
    {
        internal static StpDescendingComparer Instance = new StpDescendingComparer(new CalendarComparer());
        
        private readonly IComparer<ICalendar> _calendarComparer;

        internal StpDescendingComparer(IComparer<ICalendar> calendarComparer)
        {
            _calendarComparer = calendarComparer;
        }
        
        public int Compare((StpIndicator indicator, ICalendar calendar) x,
            (StpIndicator indicator, ICalendar calendar) y)
        {
            var compare = y.indicator.CompareTo(x.indicator);
            return compare != 0 ? compare :   _calendarComparer.Compare(x.calendar, y.calendar);
        }
    }
}