using System;

namespace Timetable
{
    public class StopSpecification
    {
        public Station Location { get; }
        public Time Time { get; }
        public DateTime OnDate { get; }

        public StopSpecification(Station location, Time time, DateTime onDate)
        {
            Location = location;
            Time = time;
            OnDate = onDate;
        }
        
    }
}