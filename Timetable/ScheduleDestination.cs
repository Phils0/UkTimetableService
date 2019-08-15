using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleDestination : ScheduleLocation, IArrival
    {
        bool IArrival.IsPublic => Arrival.IsValid;
        
        Time IArrival.Time => ((IArrival) this).IsPublic ? Arrival : WorkingArrival;

        bool IArrival.IsNextDay => ((IArrival) this).Time.IsNextDay;
        
        public Time Arrival { get; set; }

        public Time WorkingArrival { get; set; }

        public override void AddDay(Time start)
        {
            Arrival = Arrival.MakeAfterByAddingADay(start);
            WorkingArrival = WorkingArrival.MakeAfterByAddingADay(start);
        }

        public override bool IsStopAt(StopSpecification spec)
        {
            return spec.UseArrival && Station.Equals(spec.Location) && Arrival.IsSameTime(spec.Time);
        }

        public override string ToString()
        {
            return $"{Arrival} {base.ToString()}";
        }
    }
}