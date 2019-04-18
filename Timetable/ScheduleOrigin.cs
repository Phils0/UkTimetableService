using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleOrigin : ScheduleLocation, IDeparture
    {
        public Time Departure { get; set; }

        public Time WorkingDeparture { get; set; }


        public override void AddDay(Time start)
        {
            Departure = Departure.MakeAfterByAddingADay(start);
            WorkingDeparture = WorkingDeparture.MakeAfterByAddingADay(start);
        }
    }
}