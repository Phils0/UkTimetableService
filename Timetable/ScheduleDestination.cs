using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleDestination : ScheduleLocation, IArrival
    {
        public Time Arrival { get; set; }

        public Time WorkingArrival { get; set; }

        public override void AddDay(Time start)
        {
            Arrival = Arrival.MakeAfterByAddingADay(start);
            WorkingArrival = WorkingArrival.MakeAfterByAddingADay(start);
        }

        public override string ToString()
        {
            return $"{Arrival} {base.ToString()}";
        }
    }
}