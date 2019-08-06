using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleDestination : ScheduleLocation, IArrival
    {
        bool IArrival.IsPublic => Arrival.IsValid;
        
        Time IArrival.Time => ((IArrival) this).IsPublic ? Arrival : WorkingArrival;

        public Time Arrival { get; set; }

        public Time WorkingArrival { get; set; }

        public override void AddDay(Time start)
        {
            Arrival = Arrival.MakeAfterByAddingADay(start);
            WorkingArrival = WorkingArrival.MakeAfterByAddingADay(start);
        }

        public override bool IsStopAt(Station location, Time time)
        {
            return Station.Equals(location) && Arrival.Equals(time);
        }

        public override string ToString()
        {
            return $"{Arrival} {base.ToString()}";
        }
    }
}