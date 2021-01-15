using System;

namespace Timetable
{
    public interface ICalendar
    {
        /// <summary>
        /// The calendar container is active on the day
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns></returns>
        bool IsActiveOn(DateTime date);
    }
}