using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleStop : ScheduleLocation
    {
        public Time Arrival { get; set; }

        public Time WorkingArrival { get; set; }

        public Time Departure { get; set; }

        public Time WorkingDeparture { get; set; }

        public override void AddDay(Time start)
        {
            Arrival = Arrival.MakeAfterByAddingADay(start);
            WorkingArrival = WorkingArrival.MakeAfterByAddingADay(start);
            Departure = Departure.MakeAfterByAddingADay(start);
            WorkingDeparture = WorkingDeparture.MakeAfterByAddingADay(start);
        }
    }
}