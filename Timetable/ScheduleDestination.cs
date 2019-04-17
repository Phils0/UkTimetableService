using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleDestination : ScheduleLocation
    {
        public Time Arrival { get; set; }

        public Time WorkingArrival { get; set; }

        public override void AddDay(Time start)
        {
            Arrival = Arrival.MakeAfterByAddingADay(start);
            WorkingArrival = WorkingArrival.MakeAfterByAddingADay(start);
        }
    }
}