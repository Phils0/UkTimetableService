using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleStop : IScheduleLocation
    {
        public Location Location { get; set; }

        public int Sequence { get; set; }

        public Time Arrival { get; set; }

        public Time WorkingArrival { get; set; }

        public Time Departure { get; set; }

        public Time WorkingDeparture { get; set; }

        public string Platform { get; set; }

        public ISet<string> Activities { get; set; }
    }
}