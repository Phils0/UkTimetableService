using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleOrigin : ScheduleLocation, IDeparture
    {
        bool IDeparture.IsPublic => Departure.IsValid;
        
        Time IDeparture.Time => ((IDeparture) this).IsPublic ? Departure : WorkingDeparture;
        
        bool IDeparture.IsNextDay => ((IDeparture) this).Time.IsNextDay;

        public Time Departure { get; set; }

        public Time WorkingDeparture { get; set; }


        public override void AddDay(Time start)
        {
            Departure = Departure.MakeAfterByAddingADay(start);
            WorkingDeparture = WorkingDeparture.MakeAfterByAddingADay(start);
        }

        public override bool IsStopAt(StopSpecification spec)
        {
            return spec.UseDeparture && Station.Equals(spec.Location) && Departure.IsSameTime(spec.Time);
        }

        public override string ToString()
        {
            return $"{Departure} {base.ToString()}";
        }
    }
}